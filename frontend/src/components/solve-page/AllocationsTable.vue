<template>
	<div class="flex flex-col h-full">
		<div class="flex-shrink-0 mb-4">
			<Button label="Clear all" class="mr-1" @click="clearAll"></Button>
			<Button label="Clear auto-allocated" @click="clearAutoAllocated"></Button>
		</div>
		<div class="flex-1 min-h-0">
			<DataTable 
				:value="allocations" 
				:loading="false"
				class="h-full"
				scrollable
				scroll-height="flex"
				:rows="10"
				:paginator="true"
				:rows-per-page-options="[5, 10, 20, 50]"
			>
				<Column field="project" header="Project" style="min-width: 200px">
					<template #body="slotProps">
						<Select v-model="slotProps.data.project" v-tooltip.top="'Client: ' + slotProps.data.project?.client" :class="{'font-extrabold' : slotProps.data.manuallyAllocatedProject, 'supressed' : slotProps.data.project == undefined}" :options="[slotProps.data.project, ...remainingProjects].filter(x=>x)" option-label="name" filter show-clear placeholder="Select Project" @change="onProjectChange"></Select>
					</template>
				</Column>
				<Column v-for="idx of [...Array(numStudents).keys()]" :key="idx" :field="students[idx]?.name ?? 'asdf'" :header="'Student ' + (idx+1).toString()" style="min-width: 180px">
					<template #body="slotProps">
						<Select v-if="idx == 0 || slotProps.data.students[idx-1]" v-model="slotProps.data.students[idx]" :class="{'font-extrabold' : slotProps.data?.students?.[idx]?.manuallyAllocated ?? false, 'supressed' : slotProps.data.students[idx] == undefined}" :options="[slotProps.data.students[idx], ...remainingStudents].filter(x=>x)" option-label="name" filter show-clear placeholder="Select Student" @change="maintainAllocationsList"></Select>
					</template>
				</Column>
			</DataTable>
		</div>
	</div>
</template>
<script setup lang="ts">
import { computed, onMounted, watch } from 'vue';
import { ProjectDto } from '../../dtos/project-dto';
import Select, { type SelectChangeEvent } from 'primevue/select';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import type { AllocatedStudentInfo, PartialAllocation } from '../../model/PartialAllocation';
import { removeAutoAllocated } from '../../services/AllocationsServices';

const props = defineProps<{
    students: AllocatedStudentInfo[],
    projects: ProjectDto[],
    allowAdditions: boolean,
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

const emptyAllocation: PartialAllocation = {project: null, manuallyAllocatedProject: false, students: [], instanceId: 1}
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
        const canAddNewAllocations = props.allowAdditions;
        if (canAddNewAllocations) {
            allocations.value = [...(allocations.value?.filter(x => !isEmptyAllocaton(x)) ?? []), newEmptyAllocation()]
        } else {
            allocations.value = allocations.value?.filter(x => !isEmptyAllocaton(x)) ?? []
        }
        return
    }

    const lastStudentIsNotEmpty = allocations.value.length == 0 || !isEmptyAllocaton(allocations.value[l - 1])
    if (lastStudentIsNotEmpty && props.allowAdditions) {
        allocations.value = [...allocations.value ?? [], newEmptyAllocation()]
    }
}

const emit = defineEmits<{
	allocationsCleared: []
}>()

const clearAutoAllocated = () => {
	removeAutoAllocated(allocations.value ?? []);
    maintainAllocationsList()
	emit('allocationsCleared')
}

const clearAll = () => {
	allocations.value = []
	maintainAllocationsList()
	emit('allocationsCleared')
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
<style scoped>

.supressed {
	opacity: 0.5;
	transition: opacity 0.2s ease-in-out;
}

.supressed:hover {
	opacity: 1;
}

</style>
