<template>
	<AdminNavBar />
	<div class="flex flex-row flex-justify-between gap-4 p-4">
		<div>
			<SolverConfiguration v-model="solverConfig" :/>
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
import { onMounted, ref } from 'vue';
import ApiService from '../../services/ApiService';
import type { SolveRunDto } from '../../dtos/solve-run-dto';
import { useToast } from "primevue/usetoast";
import type { SolveRequestDto } from '../../dtos/solve-request-dto';
import SolverConfiguration from '../../components/SolverConfiguration.vue';
import SolveResultDisplay from '../../components/SolveResultDisplay.vue';

const defaultSolveRequest: SolveRequestDto = {
	clientLimits: [],
	preAllocations: [],
	preferenceExponent: 0.5
}

const toast = useToast();
const solverConfig = ref(defaultSolveRequest as SolveRequestDto);
const solveResult = ref(undefined as SolveRunDto | undefined)
const loading = ref(true)

onMounted(async () => {
	solveResult.value = await ApiService.get<SolveRunDto>("/solver")
	if (solveResult.value) {
		toast.add({ severity: 'success', summary: 'Success', detail: 'Got previous result', life: 1000 });
	}
	loading.value = false
})

const reset = async () => {
	solverConfig.value = defaultSolveRequest;
}

const solve = async () => {
	loading.value = true;
	solveResult.value = await ApiService.post<SolveRunDto>("/solver", undefined)
	toast.add({ severity: 'success', summary: 'Success', detail: 'Solver completed', life: 1000 });
	loading.value = false
}

</script>
