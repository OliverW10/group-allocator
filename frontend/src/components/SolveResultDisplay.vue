<template>
    <SplitButton severity="secondary" label="Full Report (.csv)" icon="i-mdi-download" class="mt-4"
    :model="downloadOptions" @click="SolverReportService.downloadFullCsvReport(solveResult!)" />
</template>
<script setup lang="ts">
import { SolveRunDto } from '../dtos/solve-run-dto';
import SolverReportService from '../services/SolverReportService';
import type { MenuItem } from 'primevue/menuitem';

const solveResult = defineModel<SolveRunDto>();

const downloadOptions: MenuItem[] = [
	{
		label: "Download Summary (.csv)",
		command: () => {
			if (solveResult.value != null) SolverReportService.downloadTable(solveResult.value)
		}
	},
	{
		label: "Full Report (.json)",
		command: () => {
			if (solveResult.value != null) SolverReportService.downloadFullJsonReport(solveResult.value)
		}
	}
]

</script>