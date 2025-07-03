<template>
    <Dialog
        v-model:visible="visible"
        modal
        header="Create New Project"
        :style="{ width: '40rem' }"
    >
        <form class="flex flex-col gap-4" @submit.prevent="handleSubmit">
            <div class="field">
                <label for="projectName" class="block mb-2 font-medium">Project Name:</label>
                <InputText
                    id="projectName"
                    v-model="formData.name"
                    required
                    placeholder="Enter project name"
                    class="w-full"
                />
            </div>
            <div class="field">
                <label for="client" class="block mb-2 font-medium">Client:</label>
                <InputText
                    id="client"
                    v-model="formData.client"
                    required
                    placeholder="Enter client name"
                    class="w-full"
                />
            </div>
            <div class="grid grid-cols-2 gap-4">
                <div class="field">
                    <label for="minStudents" class="block mb-2 font-medium">Min Students:</label>
                    <InputNumber
                        id="minStudents"
                        v-model="formData.minStudents"
                        required
                        :min="1"
                        class="w-full"
                    />
                </div>
                <div class="field">
                    <label for="maxStudents" class="block mb-2 font-medium">Max Students:</label>
                    <InputNumber
                        id="maxStudents"
                        v-model="formData.maxStudents"
                        required
                        :min="1"
                        class="w-full"
                    />
                </div>
            </div>
            <div class="grid grid-cols-2 gap-4">
                <div class="field">
                    <label for="minInstances" class="block mb-2 font-medium">Min Instances:</label>
                    <InputNumber
                        id="minInstances"
                        v-model="formData.minInstances"
                        required
                        class="w-full"
                    />
                </div>
                <div class="field">
                    <label for="maxInstances" class="block mb-2 font-medium">Max Instances:</label>
                    <InputNumber
                        id="maxInstances"
                        v-model="formData.maxInstances"
                        required
                        :min="1"
                        class="w-full"
                    />
                </div>
            </div>
            <div class="field">
                <div class="flex items-center gap-2">
                    <Checkbox
                        id="requiresNda"
                        v-model="formData.requiresNda"
                        :binary="true"
                    />
                    <label for="requiresNda" class="font-medium">Requires NDA</label>
                </div>
            </div>
        </form>
        <template #footer>
            <div class="flex justify-end gap-2">
                <Button
                    label="Cancel"
                    severity="secondary"
                    text
                    @click="handleCancel"
                />
                <Button
                    label="Create"
                    :loading="creating"
                    @click="handleSubmit"
                />
            </div>
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import Checkbox from 'primevue/checkbox';
import Button from 'primevue/button';
import ApiService from '../services/ApiService';
import type { ProjectDto } from '../dtos/project-dto';
import { useToast } from 'primevue/usetoast';

interface CreateProjectData {
    name: string;
    client: string;
    requiresNda: boolean;
    minStudents: number;
    maxStudents: number;
    minInstances: number;
    maxInstances: number;
}


interface Emits {
    (e: 'created', data: ProjectDto): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible');
const props = defineProps<{ classId: string }>();
const creating = ref(false);
const toast = useToast();
const defaultForm = {
    name: '',
    client: '',
    requiresNda: false,
    minStudents: 2,
    maxStudents: 4,
    minInstances: 0,
    maxInstances: 1
};
const formData = ref<CreateProjectData>(JSON.parse(JSON.stringify(defaultForm)));

const resetForm = () => {
    formData.value = JSON.parse(JSON.stringify(defaultForm));
};

const handleSubmit = async () => {
    creating.value = true;
    try {
        const result = await ApiService.post<ProjectDto>(`/projects?classId=${props.classId}`, formData.value);
        emit('created', result);
        toast.add({ severity: 'success', summary: 'Success', detail: 'Project created', life: 5000 });
        visible.value = false;
    } catch (error) {
        console.error(error);
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to create project', life: 10000 });
    } finally {
        creating.value = false;
    }
};

const handleCancel = () => {
    creating.value = false;
    visible.value = false;
};

// Reset form when modal is opened
watch(visible, (newValue) => {
    if (newValue) {
        resetForm();
    }
});
</script> 