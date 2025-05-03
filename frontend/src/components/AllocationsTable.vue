<template>
    <DataTable :value="allocations" :loading="false">
        <Column field="project" header="Project">
            <template #body="slotProps">
                <Select v-model="slotProps.data.project" :options="props.projects" option-label="name" filter show-clear placeholder="Select Project" @change="maintainAllocationsList"></Select>
            </template>
        </Column>
        <Column v-for="idx of [...Array(numStudents).keys()]" :key="idx" :field="students[idx]?.name ?? 'asdf'" :header="idx.toString()">
            <template #body="slotProps">
                <Select v-if="slotProps.data.students" v-model="slotProps.data.students[idx]" :options="props.students" option-label="name" filter show-clear placeholder="Select Student"></Select>
                <!-- {{slotProps.data}} -->
            </template>
        </Column>
    </DataTable>
</template>
<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
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

const numStudents = ref(3);
const allocations = defineModel<PartialAllocation[]>();

const emptyAllocation: PartialAllocation = {project: null, students: []}
const newEmptyAllocation = () => JSON.parse(JSON.stringify(emptyAllocation))
const deepCompare = (a: PartialAllocation, b: PartialAllocation) => JSON.stringify(a) === JSON.stringify(b)

const maintainAllocationsList = () => {
    if (allocations.value == undefined) {
        return
    }
    
    const l = allocations.value.length
    if (allocations.value.slice(0, l - 1).some(x => deepCompare(x, emptyAllocation))) {
        console.log('filtering')
        allocations.value = [...(allocations.value?.filter(x => !deepCompare(x, emptyAllocation)) ?? []), newEmptyAllocation()]
        return
    }

    console.log(allocations.value)
    console.log(emptyAllocation)
    if (deepCompare(allocations.value[l - 1], emptyAllocation) == false){
        console.log("pushing")
        allocations.value = [...allocations.value ?? [], newEmptyAllocation()]
    }
}



watch(allocations, maintainAllocationsList)
onMounted(() => {
    maintainAllocationsList()
})
</script>