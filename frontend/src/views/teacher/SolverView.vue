<template>
	<AdminNavBar :class-id="classId" />
	<div class="flex flex-row flex-justify-between gap-4 p-4">
		<div v-if="!loading">
			<div v-if="!allProjects || allProjects.length === 0" class="mb-4 p-4 bg-yellow-100 text-yellow-800 rounded border border-yellow-300">
				⚠️ No projects found for this class. Please add projects before running the solver.
			</div>
			<div v-if="showOutdatedWarning" class="mb-4 p-4 bg-orange-100 text-orange-800 rounded border border-orange-300">
				⚠️ The latest solve result is outdated
			</div>
			<AllocationsTable v-model="allocations" :projects="allProjects ?? []" :students="allStudentInfos" :allow-additions="true"/>
			<div class="flex flex-col gap-2 mt-4">
				<label for="preference-exponent" class="text-sm font-medium">Preference Exponent: {{ preferenceExponent.toFixed(2) }}</label>
				<Slider 
					id="preference-exponent"
					v-model="preferenceExponent" 
					:min="0.6" 
					:max="0.99" 
					:step="0.01"
					class="w-64"
				/>
				<svg :width="220" :height="120" class="mt-2 primevue-svg bg-opacity-50">
					<!-- Axes -->
					<line x1="30" y1="10" x2="30" y2="100" stroke="var(--p-surface-400, #888)" stroke-width="1" />
					<line x1="30" y1="100" x2="200" y2="100" stroke="var(--p-surface-400, #888)" stroke-width="1" />
					<!-- X axis labels -->
					<g v-for="x in 10" :key="x">
						<text :x="30 + (x-1)*17" y="115" font-size="10" text-anchor="middle" fill="var(--p-text-color-secondary, #888)">{{ x }}</text>
					</g>
					<!-- Y axis labels (0, 1, max) -->
					<text x="5" y="100" font-size="10" fill="var(--p-text-color-secondary, #888)">0</text>
					<text x="5" y="60" font-size="10" fill="var(--p-text-color-secondary, #888)">0.5</text>
					<text x="5" y="15" font-size="10" fill="var(--p-text-color-secondary, #888)">1</text>
					<!-- Curve -->
					<polyline
						:points="svgPoints"
						fill="none"
						stroke="var(--p-primary-color, #1976d2)"
						stroke-width="2"
					/>
				</svg>
			</div>
		</div>
		<div v-else>
			<ProgressSpinner />
		</div>
		<div class="p-8">
			<Divider layout="vertical" />
		</div>
		<div class="flex flex-col gap-4">
			<Button label="Run Solver" icon="i-mdi-cogs" class="w-fit" @click="solve"></Button>
		</div>
	</div>
</template>
<script setup lang="ts">
import AdminNavBar from '../../components/TeacherNavBar.vue';
import Button from 'primevue/button';
import Divider from 'primevue/divider';
import Slider from 'primevue/slider';
import { computed, onMounted, ref } from 'vue';
import ApiService from '../../services/ApiService';
import type { SolveRunDto } from '../../dtos/solve-run-dto';
import { useToast } from "primevue/usetoast";
import { SolveRequestDto } from '../../dtos/solve-request-dto';
import AllocationsTable from '../../components/AllocationsTable.vue';
import type { ProjectDto } from '../../dtos/project-dto';
import ProgressSpinner from 'primevue/progressspinner';
import type { StudentInfoAndSubmission } from '../../dtos/student-info-and-submission';
import type { AllocationDto } from '../../dtos/allocation-dto';
import type { ClientLimitsDto } from '../../dtos/client-limits-dto';
import type { AllocatedStudentInfo, PartialAllocation } from '../../model/PartialAllocation';
import type { StudentInfoDto } from '../../dtos/student-info-dto';
import { removeAutoAllocated } from '../../services/AllocationsServices';
import { useRoute } from 'vue-router';

const clientLimits = ref([] as ClientLimitsDto[])
const allocations = ref([] as PartialAllocation[])
const preferenceExponent = ref(0.85)

const toast = useToast();
const loading = ref(true)
const solved = ref(false)

const allProjects = ref(undefined as ProjectDto[] | undefined)
const allStudents = ref(undefined as StudentInfoAndSubmission[] | undefined)
const allStudentInfos = computed(() => {
	return allStudents.value?.map(student => {
		const casted = student.studentInfo as AllocatedStudentInfo;
		casted.manuallyAllocated = true
		return casted
	}) ?? []
})

const route = useRoute();
const classId = route.params.classId as string;

const showOutdatedWarning = computed(() => {
	if (!allStudents.value || allocations.value.length <= 1) return false;
	const allStudentIds = new Set(allStudents.value.map(s => s.studentInfo.studentId));
	const allocatedStudentIds = new Set(
		allocations.value.flatMap(a => a.students.map(s => s.studentId))
	);
	return allStudentIds.size > allocatedStudentIds.size;
});

const svgPoints = computed(() => {
	const m = preferenceExponent.value;
	const points: string[] = [];
	const substeps = 5;
	for (let x = 0; x <= 10 * substeps; x++) {
		// Calculate y = m^x, clamp to [0,1] for display
		const y = Math.pow(m, x);
		// Map x to [30, 200], y to [100, 10] (invert y for SVG)
		const svgX = 30 + x * 17 / substeps;
		const svgY = 100 - y * 90;
		points.push(`${svgX},${svgY}`);
	}
	return points.join(' ');
});

onMounted(async () => {
	allStudents.value = await ApiService.get<StudentInfoAndSubmission[]>(`/students?classId=${classId}`);
	allProjects.value = await ApiService.get<ProjectDto[]>(`/projects?classId=${classId}`);
	const solveResult = await ApiService.get<SolveRunDto>(`/solver?classId=${classId}`)
	if (!solveResult) {
		loading.value = false
		return;
	}
	toast.add({ severity: 'success', summary: 'Success', detail: 'Got previous result', life: 1000 });
	integrateResultToAllocations(solveResult)
	loading.value = false
})

const solve = async () => {
	loading.value = true;
	removeAutoAllocated(allocations.value ?? [])
	const solveRequest: SolveRequestDto = {
		clientLimits: clientLimits.value,
		preAllocations: (allocations.value as AllocationDto[]).filter(x => x.project != null || x.students.length > 0),
		preferenceExponent: preferenceExponent.value,
		classId: parseInt(classId),
	}
	const solveResult = await ApiService.post<SolveRunDto>(`/solver`, solveRequest)
	if (!solveResult) {
		toast.add({ severity: 'error', summary: 'Failed', detail: 'Failed to find solution for student allocations', life: 3000 });
		loading.value = false
		return
	}
	toast.add({ severity: 'success', summary: 'Success', detail: 'Solver completed', life: 1000 });
	integrateResultToAllocations(solveResult)
	loading.value = false
	solved.value = true
}

const integrateResultToAllocations = (solveResult: SolveRunDto) => {
	if (solveResult == undefined) {
		return
	}
	for (const autoAllocation of solveResult.projects) {
		const existingWithMatchingProjects = allocations.value.filter(x => x.project?.id == autoAllocation.project.id && x.instanceId == autoAllocation.instanceId);
		// There was a manual allocation with a matching project
		if (existingWithMatchingProjects.length) {
			const relevantExisting = existingWithMatchingProjects[0]
			addMissingStudents(autoAllocation, relevantExisting);
			continue
		}
		
		const existingWithMatchingStudents = allocations.value.filter(a => a.students.every(s => autoAllocation.students.some(s2 => s2.studentId == s.studentId)))
		// There was a manual allocation with matching students
		if (existingWithMatchingStudents.length) {
			const relevantExisting = existingWithMatchingStudents[0]
			addMissingStudents(autoAllocation, relevantExisting)
			relevantExisting.project = autoAllocation.project
			relevantExisting.manuallyAllocatedProject = false
			continue;
		}

		// No matching manual allocation, create entirely new one
		allocations.value.push({
			manuallyAllocatedProject: false,
			project: autoAllocation.project,
			students: autoAllocation.students.map(x => studentInfoToAllocated(x)),
			instanceId: autoAllocation.instanceId,
		})
	}

}

// Adds students to a project if they are not already on it
const addMissingStudents = (autoAllocation: AllocationDto, relevantExisting: PartialAllocation) => {
	const newStudents = autoAllocation.students.filter(s => !relevantExisting.students.some(s2 => s2.studentId == s.studentId));
	for (const newStudent of newStudents) {
		relevantExisting.students.push(studentInfoToAllocated(newStudent));
	}
}
const studentInfoToAllocated = (i: StudentInfoDto) => {
	const casted = i as AllocatedStudentInfo
	casted.manuallyAllocated = false
	return casted
}

</script>

<style scoped>
.primevue-svg {
	background: var(--p-surface-0, #fff);
	border: 1px solid var(--p-surface-border, #d1d5db);
	border-radius: 0.375rem;
	display: block;
}
</style>
