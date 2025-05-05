<template>
	<AdminNavBar />
	<div class="flex flex-row flex-justify-between gap-4 p-4">
		<div v-if="!loading">
			<Button label="Reset" @click="reset"></Button>
			<AllocationsTable v-model="allocations" :projects="allProjects ?? []" :students="allStudentInfos"/>
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
			<SolveResultDisplay v-model="solveResult" />
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
import SolveResultDisplay from '../../components/SolveResultDisplay.vue';
import AllocationsTable, { type AllocatedStudentInfo, type PartialAllocation } from '../../components/AllocationsTable.vue';
import type { ProjectDto } from '../../dtos/project-dto';
import ProgressSpinner from 'primevue/progressspinner';
import type { StudentInfoAndSubmission } from '../../dtos/student-info-and-submission';
import type { AllocationDto } from '../../dtos/allocation-dto';
import type { ClientLimitsDto } from '../../dtos/client-limits-dto';

const clientLimits = ref([] as ClientLimitsDto[])
const allocations = ref([] as PartialAllocation[])
const preferenceExponent = ref(0.5)

const toast = useToast();
const solveResult = ref(undefined as SolveRunDto | undefined)
const loading = ref(true)

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
	solveResult.value = await ApiService.get<SolveRunDto>("/solver")
	if (solveResult.value) {
		toast.add({ severity: 'success', summary: 'Success', detail: 'Got previous result', life: 1000 });
	}
	allStudents.value = await ApiService.get<StudentInfoAndSubmission[]>("/students");
	allProjects.value = await ApiService.get<ProjectDto[]>("/projects");
	loading.value = false
})

const resetAll = async () => {
	allocations.value = []
}

const resetAutomatic = async () => {
	// 
}

const solve = async () => {
	loading.value = true;
	const solveRequest: SolveRequestDto = {
		clientLimits: clientLimits.value,
		preAllocations: allocations.value as AllocationDto[],
		preferenceExponent: preferenceExponent.value
	}
	solveResult.value = await ApiService.post<SolveRunDto>("/solver", solveRequest)
	toast.add({ severity: 'success', summary: 'Success', detail: 'Solver completed', life: 1000 });
	loading.value = false
}

</script>
