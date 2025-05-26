<template>
	<AdminNavBar />
	<div class="flex flex-row flex-justify-between gap-4 p-4">
		<div v-if="!loading">
			<AllocationsTable v-model="allocations" :projects="allProjects ?? []" :students="allStudentInfos" :allow-additions="true"/>
			<!-- <SolverConfiguration v-model="solverConfig.value"></SolverConfiguration> -->
		</div>
		<div v-else>
			<ProgressSpinner />
		</div>
		<div class="p-8">
			<Divider layout="vertical" />
		</div>
		<div>
			<Button label="Run Solver" icon="i-mdi-cogs" class="w-fit" @click="solve"></Button>
		</div>
	</div>
</template>
<script setup lang="ts">
import AdminNavBar from '../../components/AdminNavBar.vue';
import Button from 'primevue/button';
import Divider from 'primevue/divider';
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

const clientLimits = ref([] as ClientLimitsDto[])
const allocations = ref([] as PartialAllocation[])
const preferenceExponent = ref(0.5)

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

onMounted(async () => {
	const solveResult = await ApiService.get<SolveRunDto>("/solver")
	if (solveResult) {
		toast.add({ severity: 'success', summary: 'Success', detail: 'Got previous result', life: 1000 });
	}
	integrateResultToAllocations(solveResult)
	allStudents.value = await ApiService.get<StudentInfoAndSubmission[]>("/students");
	allProjects.value = await ApiService.get<ProjectDto[]>("/projects");
	loading.value = false
})

const solve = async () => {
	loading.value = true;
	removeAutoAllocated(allocations.value ?? [])
	const solveRequest: SolveRequestDto = {
		clientLimits: clientLimits.value,
		preAllocations: (allocations.value as AllocationDto[]).filter(x => x.project != null || x.students.length > 0),
		preferenceExponent: preferenceExponent.value
	}
	const solveResult = await ApiService.post<SolveRunDto>("/solver", solveRequest)
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
		const existingWithMatchingProjects = allocations.value.filter(x => x.project?.id == autoAllocation.project.id)
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
			instanceId: 1, // Can't manually allocate multiple instances of the same project for now
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
