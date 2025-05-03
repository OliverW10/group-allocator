<template>
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
import type { SolveRequestDto } from '../dtos/solve-request-dto';

const solveRequest = defineModel<SolveRequestDto>();

</script>