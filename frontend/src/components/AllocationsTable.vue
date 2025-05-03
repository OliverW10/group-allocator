<template>
    <DataTable :value="allocations" :loading="false">
        <Column field="project" header="Project">
            <template #body="slotProps">
                <Select v-model="slotProps.data.project" :options="props.projects" option-label="name" filter show-clear placeholder="Select Project" @change="maintainAllocationsList"></Select>
            </template>
        </Column>
        <Column v-for="idx of [...Array(numStudents).keys()]" :key="idx" :field="students[idx]?.name ?? 'asdf'" :header="idx.toString()">
            <template #body="slotProps">
                <Select v-if="slotProps.data.students" v-model="slotProps.data.students[idx]" :options="props.students" option-label="name" filter show-clear placeholder="Select Student" @change="maintainAllocationsList"></Select>
                <!-- {{slotProps.data}} -->
            </template>
        </Column>
    </DataTable>
</template>
<script setup lang="ts">
import { computed, onMounted, watch } from 'vue';
import { ProjectDto } from '../dtos/project-dto';
import { StudentInfoDto } from '../dtos/student-info-dto';
import Select from 'primevue/select';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';

interface PartialAllocation {
    project: ProjectDto | null,
    students: StudentInfoDto[],
}

const props = defineProps<{
    students: StudentInfoDto[],
    projects: ProjectDto[],
}>();

const allocations = defineModel<PartialAllocation[]>();
const numStudents = computed(() => {
    const lengths = allocations.value?.map(x => x.students.length) ?? [0]
    const maxLength = lengths.length != 0 ? Math.max(...lengths) : 0
    // TODO: constrain based on actual limit
    return Math.min(maxLength + 1, 6)
})
// TODO: fix page shift when selecting project

const emptyAllocation: PartialAllocation = {project: null, students: []}
const newEmptyAllocation = () => structuredClone(emptyAllocation)
const isEmptyAllocaton = (allocation: PartialAllocation) => {
    return allocation.project == null && (allocation.students == null || allocation.students?.every(x => x == null))
}

const maintainAllocationsList = () => {
    if (allocations.value == undefined) {
        return
    }
    
    const l = allocations.value.length
    if (allocations.value.slice(0, l - 1).some(x => isEmptyAllocaton(x))) {
        console.log('filtering')
        allocations.value = [...(allocations.value?.filter(x => !isEmptyAllocaton(x)) ?? []), newEmptyAllocation()]
        return
    }

    if (allocations.value.length == 0 || !isEmptyAllocaton(allocations.value[l - 1])){
        allocations.value = [...allocations.value ?? [], newEmptyAllocation()]
    }
}

watch(allocations, maintainAllocationsList)
onMounted(() => {
    maintainAllocationsList()
})
</script>