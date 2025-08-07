<template>
	<TeacherNavBar :class-id="classId" />
	<div class="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50 p-6">
		<div class="mx-auto">
			<!-- Header Section -->
			<div class="mb-8">
				<h1 class="text-3xl font-bold text-gray-900 mb-2">Group Allocation Solver</h1>
				<p class="text-gray-600">Configure and run the allocation algorithm for your class</p>
			</div>

			<div class="flex gap-8">
				<!-- Main Content Area -->
				<div class="flex-1">
					<div v-if="!loading">
						<!-- Warning Messages -->
						<WarningMessage 
							:show="!allProjects || allProjects.length === 0"
							severity="warning"
							title="No Projects Found"
							message="Please add projects before running the solver."
						/>

						<WarningMessage 
							:show="showOutdatedWarning"
							severity="warning"
							title="Outdated Results"
							message="The latest solve result is outdated."
						/>

						<!-- Allocations Table Card -->
						<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-6">
							<h2 class="text-xl font-semibold text-gray-900 mb-4">Current Allocations</h2>
							<AllocationsTable v-model="allocations" :projects="allProjects ?? []" :students="allStudentInfos" :allow-additions="true" @allocations-cleared="onAllocationsCleared"/>
						</div>

						<!-- Preference Configuration Card -->
						<PreferenceCurve v-model="preferenceExponent" />

						<!-- Client Limits Card -->
						<ClientLimits v-model="clientLimits" :all-clients="allClients" />
					</div>
					
					<!-- Loading State -->
					<div v-else class="flex items-center justify-center py-12">
						<div class="text-center">
							<ProgressSpinner class="mb-4" />
							<p class="text-gray-600">Running solver...</p>
						</div>
					</div>
				</div>

				<!-- Sidebar -->
				<div class="w-80">
					<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 sticky top-6">
						<h2 class="text-xl font-semibold text-gray-900 mb-6">Solver Actions</h2>
						
						<div class="space-y-4">
							<Button 
								label="Run Solver" 
								icon="i-mdi-cogs" 
								class="w-full h-12 text-base font-medium bg-blue-600 hover:bg-blue-700"
								:loading="loading"
								@click="solve"
							/>
							
							<Button 
								label="Download Solution"
								icon="i-mdi-download" 
								class="w-full h-12 text-base font-medium bg-green-600 hover:bg-green-700"
								:disabled="!solved"
								@click="downloadReport"
							/>
						</div>

						<!-- Status Indicator -->
						<div v-if="solved" class="mt-6 p-4 bg-green-50 border border-green-200 rounded-lg">
							<div class="flex items-center gap-2">
								<div class="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
								<span class="text-sm font-medium text-green-800">Solver completed successfully</span>
							</div>
						</div>

						<!-- Histogram Chart -->
						<div v-if="solved" class="mt-6">
							<h3 class="text-lg font-semibold text-gray-900 mb-4">Preference Distribution</h3>
							<PreferenceHistogram :histogram="histogram" />
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>
<script setup lang="ts">
import TeacherNavBar from '../../components/TeacherNavBar.vue';
import Button from 'primevue/button';
import { computed, onMounted, ref } from 'vue';
import ApiService from '../../services/ApiService';
import type { SolveRunDto } from '../../dtos/solve-run-dto';
import { useToast } from "primevue/usetoast";
import { SolveRequestDto } from '../../dtos/solve-request-dto';
import AllocationsTable from '../../components/AllocationsTable.vue';
import PreferenceHistogram from '../../components/PreferenceHistogram.vue';
import PreferenceCurve from '../../components/PreferenceCurve.vue';
import WarningMessage from '../../components/WarningMessage.vue';
import ClientLimits from '../../components/ClientLimits.vue';
import type { ProjectDto } from '../../dtos/project-dto';
import ProgressSpinner from 'primevue/progressspinner';
import type { StudentInfoAndSubmission } from '../../dtos/student-info-and-submission';
import type { AllocationDto } from '../../dtos/allocation-dto';
import type { ClientLimitsDto } from '../../dtos/client-limits-dto';
import type { AllocatedStudentInfo, PartialAllocation } from '../../model/PartialAllocation';
import type { StudentInfoDto } from '../../dtos/student-info-dto';
import { removeAutoAllocated } from '../../services/AllocationsServices';
import { useRoute } from 'vue-router';
import SolverReportService from '../../services/SolverReportService';

import type { ClientDto } from '../../dtos/client-dto';

const clientLimits = ref([] as ClientLimitsDto[])
const allocations = ref([] as PartialAllocation[])
const preferenceExponent = ref(0.85)
const histogram = ref<number[]>([])

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

const allClients = ref<{ id: number, name: string }[]>([]);

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

onMounted(async () => {
	allStudents.value = await ApiService.get<StudentInfoAndSubmission[]>(`/students?classId=${classId}`);
	allProjects.value = await ApiService.get<ProjectDto[]>(`/projects?classId=${classId}`);
	allClients.value = await ApiService.get<ClientDto[]>(`/projects/clients?classId=${classId}`);
	const solveResult = await ApiService.get<SolveRunDto>(`/solver?classId=${classId}`)
	if (!solveResult) {
		loading.value = false
		return;
	}
	solved.value = true
	toast.add({ severity: 'success', summary: 'Success', detail: 'Got previous result', life: 1000 });
	integrateResultToAllocations(solveResult)
	preferenceExponent.value = solveResult.preferenceExponent
	clientLimits.value = solveResult.clientLimits
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

const downloadReport = async () => {
	if (!solved.value) {
		toast.add({ severity: 'warn', summary: 'Warning', detail: 'No solve result available to download', life: 3000 });
		return;
	}
	
	// Convert allocations back to SolveRunDto format
	const solveRunDto: SolveRunDto = {
		id: Date.now(), // Use timestamp as ID
		ranAt: new Date(),
		projects: allocations.value.map(allocation => ({
			project: allocation.project!,
			students: allocation.students.map(student => ({
				studentId: student.studentId,
				name: student.name,
				email: student.email,
				isVerified: student.isVerified
			})),
			instanceId: allocation.instanceId
		})),
		preferenceExponent: preferenceExponent.value,
		clientLimits: clientLimits.value,
		histogram: histogram.value,
	};
	
	try {
		await SolverReportService.downloadFullCsvReport(solveRunDto);
		toast.add({ severity: 'success', summary: 'Success', detail: 'Report downloaded successfully', life: 2000 });
	} catch {
		toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to download report', life: 3000 });
	}
}

const integrateResultToAllocations = (solveResult: SolveRunDto) => {
	if (solveResult == undefined) {
		return
	}
	// Store histogram data
	histogram.value = solveResult.histogram || []
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

const onAllocationsCleared = () => {
	solved.value = false
	histogram.value = []
}

</script>


