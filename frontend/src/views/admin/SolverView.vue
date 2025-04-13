<template>
    <AdminNavBar />
    <div class="px-4 py-2 mt-4 flex flex-col gap-4">
        <h1 class="heading">Solver</h1>
        <Divider style="margin: 0;" />
        <Button label="Run Solver" icon="i-mdi-cogs" class="w-fit" @click="solve"></Button>
        <Listbox v-model="selectedRun" :options="solveRuns" option-label="ranAt" class="w-fit" />
        <h3 v-if="selectedRun == undefined">Select a run to view allocations</h3>
        <DataTable :value="selectedRun?.projects ?? []" :loading="loading" :paginator="true" :rows="30"
            :rows-per-page-options="[30, 100]">
            <Column field="projectId" header="Project"></Column>
            <Column field="studentIds" header="Students"></Column>
        </DataTable>
        <a :href="ApiService.makeUrl(`/solver/export/${selectedRun?.id}`).toString()" download
            :disabled="selectedRun == undefined">
            <Button severity="secondary" label="Download Results" icon="i-mdi-download" />
        </a>
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

const toast = useToast();

const solveRuns = ref([] as SolveRunDto[]);
const selectedRun = ref(undefined as SolveRunDto | undefined);
const loading = ref(true);

onMounted(async () => {
    const runs = await ApiService.get<SolveRunDto[]>("/solver");
    solveRuns.value = runs.slice(0, 10)
    loading.value = false;
});

const solve = async () => {
    loading.value = true;
    solveRuns.value = (await ApiService.post<SolveRunDto[]>("/solver", undefined)).slice(0, 10)
    // TODO: error handling
    toast.add({ severity: 'success', summary: 'Success', detail: 'Loaded previous submission', life: 3000 });
    loading.value = false;
}

</script>