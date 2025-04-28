<template>
    <h2>Student Allocations</h2>
    <DataTable edit-mode="cell" :value="solveRequest?.preAllocations ?? []" :loading="false">
        <Column field="project" header="Project">
            <template #body="slotProps">
                dropdown...
                {{ slotProps.data.project.name }}
            </template>
        </Column>
        <Column field="studentIds" header="Students">
            <template #body="slotProps">
                dropdown...
                {{slotProps.data}}
            </template>
        </Column>
    </DataTable>
    <Button label="+"></Button>
    <h2>Client Limits</h2>
    <DataTable edit-mode="cell" :value="solveRequest?.clientLimits ?? []" :loading="false">
        <Column field="project" header="Project">
            <template #body="slotProps">
                dropdown...
                {{ slotProps.data.project.name }}
            </template>
        </Column>
        <Column field="studentIds" header="Students">
            <template #body="slotProps">
                {{slotProps.data}}
            </template>
        </Column>
    </DataTable>
    <h2>Weightings</h2>
</template>
<script setup lang="ts">
import { computed } from 'vue';
import { SolveRequestDto } from '../dtos/solve-request-dto';
import type { SolveRunDto } from '../dtos/solve-run-dto';
import type { AllocationDto } from '../dtos/allocation-dto';

const solveRequest = defineModel<SolveRequestDto>();
const props = defineProps<{
    result: SolveRunDto
}>();

const allocations: AllocationDto[] = computed(() => {
    get: 
})

</script>