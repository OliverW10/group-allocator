<template>
	<AdminNavBar />
	<div class="px-4 py-2 mt-4 flex flex-col gap-4">
		<h1 class="heading">Solver</h1>
		<Divider style="margin: 0;" />
		<Button label="Run Solver" icon="i-mdi-cogs" class="w-fit" @click="solve"></Button>
		<Listbox v-model="selectedRun" :options="solveRuns" option-label="ranAt" class="w-fit" :multiple="false" />
		<h3 v-if="selectedRun == undefined">Select a run to view allocations</h3>
		<div v-else>
			<DataTable edit-mode="cell" :value="selectedRun?.projects ?? []" :loading="loading" :paginator="true" :rows="10"
				:rows-per-page-options="[5, 10, 20, 50]">
				<Column field="project" header="Project">
					<template #body="slotProps">
						{{ slotProps.data.project.name }}
					</template>
				</Column>
				<Column field="studentIds" header="Students">
					<template #body="slotProps">
						{{slotProps.data.students.map((s: StudentInfoDto) => `${s.name} (${s.email})`).join(', ')}}
					</template>
				</Column>
			</DataTable>
			<SplitButton severity="secondary" label="Full Report (.csv)" icon="i-mdi-download" class="mt-4"
				:model="downloadOptions" @click="SolverReportService.downloadFullCsvReport(selectedRun)" />
		</div>
	</div>
</template>
<script setup lang="ts">
import AdminNavBar from '../../components/AdminNavBar.vue';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Listbox from 'primevue/listbox';
import Button from 'primevue/button';
import Divider from 'primevue/divider';
import { onMounted, ref } from 'vue';
import ApiService from '../../services/ApiService';
import type { SolveRunDto } from '../../dtos/solve-run-dto';
import { useToast } from "primevue/usetoast";
import type { StudentInfoDto } from '../../dtos/student-info-dto';
import { SplitButton } from 'primevue';
import SolverReportService from '../../services/SolverReportService';
import type { MenuItem } from 'primevue/menuitem';

const toast = useToast();

const solveRuns = ref([] as SolveRunDto[]);
const selectedRun = ref(null as SolveRunDto | null);
const loading = ref(true);

const downloadOptions: MenuItem[] = [
	{
		label: "Download Summary (.csv)",
		command: () => {
			if (selectedRun.value != null) SolverReportService.downloadTable(selectedRun.value)
		}
	},
	{
		label: "Full Report (.json)",
		command: () => {
			if (selectedRun.value != null) SolverReportService.downloadFullJsonReport(selectedRun.value)
		}
	}
]

onMounted(async () => {
	const runs = await ApiService.get<SolveRunDto[]>("/solver");
	solveRuns.value = runs.slice(0, 10)
	loading.value = false;
});

const solve = async () => {
	loading.value = true;
	solveRuns.value = (await ApiService.post<SolveRunDto[]>("/solver", undefined)).slice(0, 10)
	// TODO: error handling
	toast.add({ severity: 'success', summary: 'Success', detail: 'Solver completed', life: 3000 });
	loading.value = false;
}

</script>
