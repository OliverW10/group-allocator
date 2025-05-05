<template>
	<div>
		<Button label="Clear all" class="mr-1" @click="clearAll"></Button>
		<Button label="Clear auto-allocated" @click="clearAutoAllocated"></Button>
	</div>
    <DataTable :value="allocations" :loading="false">
        <Column field="project" header="Project">
            <template #body="slotProps">
                <Select v-model="slotProps.data.project" :class="{'font-extrabold' : slotProps.data.manuallyAllocatedProject}" :options="[slotProps.data.project, ...remainingProjects].filter(x=>x)" option-label="name" filter show-clear placeholder="Select Project" @change="onProjectChange"></Select>
            </template>
        </Column>
        <Column v-for="idx of [...Array(numStudents).keys()]" :key="idx" :field="students[idx]?.name ?? 'asdf'" :header="'Student ' + idx.toString()">
            <template #body="slotProps">
                <Select v-if="slotProps.data.students" v-model="slotProps.data.students[idx]" :class="{'font-extrabold' : slotProps.data?.students?.[idx]?.manuallyAllocated ?? false}" :options="[slotProps.data.students[idx], ...remainingStudents].filter(x=>x)" option-label="name" filter show-clear placeholder="Select Student" @change="maintainAllocationsList"></Select>
            </template>
        </Column>
    </DataTable>
</template>
<script setup lang="ts">
import { computed, onMounted, watch } from 'vue';
import { ProjectDto } from '../dtos/project-dto';
import Select, { type SelectChangeEvent } from 'primevue/select';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import { removeAutoAllocated, type AllocatedStudentInfo, type PartialAllocation } from '../model/PartialAllocation';

const props = defineProps<{
    students: AllocatedStudentInfo[],
    projects: ProjectDto[],
}>();
const allocations = defineModel<PartialAllocation[]>();

const remainingStudents = computed(() => {
    const usedStudents = allocations.value?.flatMap(aloc => aloc.students) ?? []
    return props.students.filter(stu => !usedStudents.includes(stu))
})

const remainingProjects = computed(() => {
    const usedProjects = allocations.value?.map(aloc => aloc.project).filter(x => x) ?? []
    return props.projects.filter(proj => !usedProjects.includes(proj))
})

const numStudents = computed(() => {
    const lengths = allocations.value?.map(x => x.students.length) ?? [0]
    const maxLength = lengths.length != 0 ? Math.max(...lengths) : 0
    // TODO: constrain based on actual limit
    return Math.min(maxLength + 1, 6)
})
// TODO: fix page shift when selecting project

const emptyAllocation: PartialAllocation = {project: null, manuallyAllocatedProject: false, students: []}
const newEmptyAllocation = () => structuredClone(emptyAllocation)
const isEmptyAllocaton = (allocation: PartialAllocation) => {
    return allocation.project == null && (allocation.students == null || allocation.students?.every(x => x == null))
}

const maintainAllocationsList = () => {
    if (allocations.value == undefined) {
        return
    }
    
	// if any students are null, remove them
	for (const allocation of allocations.value) {
		if (allocation.students.some(x=>x==null)){ // prevent updates when nothing to change
			allocation.students = allocation.students.filter(x => x != null)
		}
	}

	// if any allocation that isn't the last one is empty, filter them out
    const l = allocations.value.length
    if (allocations.value.slice(0, l - 1).some(x => isEmptyAllocaton(x))) {
        console.log('filtering')
        allocations.value = [...(allocations.value?.filter(x => !isEmptyAllocaton(x)) ?? []), newEmptyAllocation()]
        return
    }

	// if the last allocation is not empty, add a new one
    if (allocations.value.length == 0 || !isEmptyAllocaton(allocations.value[l - 1])){
        allocations.value = [...allocations.value ?? [], newEmptyAllocation()]
    }
}

const clearAutoAllocated = () => {
    removeAutoAllocated(allocations.value)
	maintainAllocationsList()
}

const clearAll = () => {
	allocations.value = []
	maintainAllocationsList()
}

const onProjectChange = (event: SelectChangeEvent) => {
    for (const aloc of allocations.value?.filter(x => x.project == event.value) ?? []) {
        aloc.manuallyAllocatedProject = true
    }
    maintainAllocationsList()
}

watch(allocations, maintainAllocationsList)
onMounted(() => {
    maintainAllocationsList()
})
</script>
