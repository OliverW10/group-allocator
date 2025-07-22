<template>
	<AdminNavBar :class-id="classId" />
	<div class="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50 p-6">
		<div class="max-w-7xl mx-auto">
			<!-- Header Section -->
			<div class="mb-8">
				<h1 class="text-3xl font-bold text-gray-900 mb-2">Group Allocation Solver</h1>
				<p class="text-gray-600">Configure and run the allocation algorithm for your class</p>
			</div>

			<div class="flex flex-row gap-8">
				<!-- Main Content Area -->
				<div class="flex-1">
					<div v-if="!loading">
						<!-- Warning Messages -->
						<div v-if="!allProjects || allProjects.length === 0" class="mb-6 p-4 bg-amber-50 border border-amber-200 rounded-xl shadow-sm">
							<div class="flex items-center gap-3">
								<div class="flex-shrink-0">
									<svg class="w-5 h-5 text-amber-600" fill="currentColor" viewBox="0 0 20 20">
										<path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
									</svg>
								</div>
								<div>
									<h3 class="text-sm font-medium text-amber-800">No Projects Found</h3>
									<p class="text-sm text-amber-700 mt-1">Please add projects before running the solver.</p>
								</div>
							</div>
						</div>

						<div v-if="showOutdatedWarning" class="mb-6 p-4 bg-orange-50 border border-orange-200 rounded-xl shadow-sm">
							<div class="flex items-center gap-3">
								<div class="flex-shrink-0">
									<svg class="w-5 h-5 text-orange-600" fill="currentColor" viewBox="0 0 20 20">
										<path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
									</svg>
								</div>
								<div>
									<h3 class="text-sm font-medium text-orange-800">Outdated Results</h3>
									<p class="text-sm text-orange-700 mt-1">The latest solve result is outdated.</p>
								</div>
							</div>
						</div>

						<!-- Allocations Table Card -->
						<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-6">
							<h2 class="text-xl font-semibold text-gray-900 mb-4">Current Allocations</h2>
							<AllocationsTable v-model="allocations" :projects="allProjects ?? []" :students="allStudentInfos" :allow-additions="true" @allocations-cleared="onAllocationsCleared"/>
						</div>

						<!-- Preference Configuration Card -->
						<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<h2 class="text-xl font-semibold text-gray-900 mb-4">Preference Configuration</h2>
							<div class="flex flex-col gap-4">
								<div>
									<label for="preference-exponent" class="block text-sm font-medium text-gray-700 mb-2">
										Preference Exponent: <span class="text-blue-600 font-semibold">{{ preferenceExponent.toFixed(2) }}</span>
									</label>
									<Slider 
										id="preference-exponent"
										v-model="preferenceExponent" 
										:min="0.6" 
										:max="0.99" 
										:step="0.01"
										class="w-full max-w-md"
									/>
								</div>
								
								<!-- Preference Curve Visualization -->
								<div class="mt-4">
									<h3 class="text-sm font-medium text-gray-700 mb-3">Preference Curve Preview</h3>
									<div class="bg-gray-50 rounded-lg p-4 border border-gray-200">
										<svg :width="280" :height="140" class="mx-auto">
											<!-- Background grid -->
											<defs>
												<pattern id="grid" width="17" height="18" patternUnits="userSpaceOnUse">
													<path d="M 17 0 L 0 0 0 18" fill="none" stroke="#e5e7eb" stroke-width="0.5"/>
												</pattern>
											</defs>
											<rect width="100%" height="100%" fill="url(#grid)" />
											
											<!-- Axes -->
											<line x1="40" y1="20" x2="40" y2="120" stroke="#9ca3af" stroke-width="2" />
											<line x1="40" y1="120" x2="260" y2="120" stroke="#9ca3af" stroke-width="2" />
											
											<!-- X axis labels -->
											<g v-for="x in 10" :key="x">
												<text :x="40 + (x-1)*22" y="135" font-size="11" text-anchor="middle" fill="#6b7280" font-weight="500">{{ x }}</text>
											</g>
											
											<!-- Y axis labels -->
											<text x="15" y="120" font-size="11" fill="#6b7280" font-weight="500">0</text>
											<text x="15" y="70" font-size="11" fill="#6b7280" font-weight="500">0.5</text>
											<text x="15" y="25" font-size="11" fill="#6b7280" font-weight="500">1</text>
											
											<!-- Curve -->
											<polyline
												:points="svgPoints"
												fill="none"
												stroke="#3b82f6"
												stroke-width="3"
												stroke-linecap="round"
												stroke-linejoin="round"
											/>
											
											<!-- Curve gradient effect -->
											<defs>
												<linearGradient id="curveGradient" x1="0%" y1="0%" x2="100%" y2="0%">
													<stop offset="0%" style="stop-color:#3b82f6;stop-opacity:0.8" />
													<stop offset="100%" style="stop-color:#1d4ed8;stop-opacity:0.6" />
												</linearGradient>
											</defs>
										</svg>
									</div>
									<p class="text-xs text-gray-500 mt-2 text-center">
										Higher values give more weight to student preferences
									</p>
								</div>
							</div>
						</div>
						<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mt-6">
							<h2 class="text-xl font-semibold text-gray-900 mb-4">Client Limits</h2>
							<DataTable :value="clientLimits" class="w-full" :rows="10" editMode="cell" :rowClass="rowClassForClientLimit">
								<Column field="clientId" header="Client" style="min-width: 200px">
									<template #body="slotProps">
										<Dropdown v-model="slotProps.data.clientId" :options="allClients" option-label="name" option-value="id" placeholder="Select Client" class="w-full" />
									</template>
								</Column>
								<Column field="minProjects" header="Min Projects" style="min-width: 120px">
									<template #body="slotProps">
										<InputNumber v-model="slotProps.data.minProjects" :min="0" class="w-full" />
									</template>
								</Column>
								<Column field="maxProjects" header="Max Projects" style="min-width: 120px">
									<template #body="slotProps">
										<InputNumber v-model="slotProps.data.maxProjects" :min="0" class="w-full" />
									</template>
								</Column>
								<Column header="Actions" style="min-width: 80px">
									<template #body="slotProps">
										<Button icon="i-mdi-delete" severity="danger" text @click="removeClientLimit(slotProps.index)" />
									</template>
								</Column>
								<template #footer>
									<Button label="Add Client Limit" icon="i-mdi-plus" class="mt-2" @click="addClientLimit" />
								</template>
							</DataTable>
							<div v-if="invalidClientLimits.length > 0" class="mt-2 text-red-600 text-sm">
								<ul>
									<li v-for="(lim, idx) in invalidClientLimits" :key="idx">
										Client {{ allClients.find(c => c.id === lim.clientId)?.name || lim.clientId }}: Max Projects must be greater than or equal to Min Projects.
									</li>
								</ul>
							</div>
						</div>
					</div>
					
					<!-- Loading State -->
					<div v-else class="flex items-center justify-center py-12">
						<div class="text-center">
							<ProgressSpinner class="mb-4" />
							<p class="text-gray-600">Loading solver configuration...</p>
						</div>
					</div>
				</div>

				<!-- Sidebar -->
				<div class="w-80 flex-shrink-0">
					<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 sticky top-6">
						<h2 class="text-xl font-semibold text-gray-900 mb-6">Solver Actions</h2>
						
						<div class="space-y-4">
							<Button 
								label="Run Solver" 
								icon="i-mdi-cogs" 
								class="w-full h-12 text-base font-medium bg-blue-600 hover:bg-blue-700 border-blue-600 hover:border-blue-700"
								@click="solve"
								:loading="loading"
							/>
							
							<Button 
								label="Download Solution"
								icon="i-mdi-download" 
								class="w-full h-12 text-base font-medium bg-green-600 hover:bg-green-700 border-green-600 hover:border-green-700"
								@click="downloadReport"
								:disabled="!solved"
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
import AdminNavBar from '../../components/TeacherNavBar.vue';
import Button from 'primevue/button';
import Slider from 'primevue/slider';
import { computed, onMounted, ref } from 'vue';
import ApiService from '../../services/ApiService';
import type { SolveRunDto } from '../../dtos/solve-run-dto';
import { useToast } from "primevue/usetoast";
import { SolveRequestDto } from '../../dtos/solve-request-dto';
import AllocationsTable from '../../components/AllocationsTable.vue';
import PreferenceHistogram from '../../components/PreferenceHistogram.vue';
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
import Dropdown from 'primevue/dropdown';
import InputNumber from 'primevue/inputnumber';
import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
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

const addClientLimit = () => {
	clientLimits.value.push({ clientId: allClients.value[0]?.id ?? 0, minProjects: 0, maxProjects: 1 });
};
const removeClientLimit = (idx: number) => {
	clientLimits.value.splice(idx, 1);
};

const invalidClientLimits = computed(() =>
  clientLimits.value.filter(lim => lim.maxProjects < lim.minProjects)
);

const rowClassForClientLimit = (data: ClientLimitsDto) => {
  return { 'bg-red-100': data.maxProjects < data.minProjects };
};

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
		// Map x to [40, 260], y to [120, 20] (invert y for SVG)
		const svgX = 40 + x * 22 / substeps;
		const svgY = 120 - y * 100;
		points.push(`${svgX},${svgY}`);
	}
	return points.join(' ');
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
		histogram: []
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


