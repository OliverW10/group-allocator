<template>
    <TeacherNavBar :class-id="classId" />
    <div class="px-4 py-2 mt-4 flex flex-col gap-4">
        <div class="flex justify-between items-center">
            <h1 class="heading">Projects</h1>
        </div>
        <Divider style="margin: 0;" />
        <div class="flex justify-between items-end">
            <div class="flex">
                <FileUploader label="Import Projects" @projects-changed="uploadProjects">
                        Please upload a csv file with the following format and no header
                        <table>
                            <thead>
                                <tr class="">
                                    <th
                                        v-for="col of ['project_name', 'client', 'min_students', 'max_students', 'requires_nda', 'min_instances', 'max_instances']"
                                        :key="col"
                                        class="border border-black p-2"
                                    >{{ col }}</th>
                                </tr>
                            </thead>
                        </table>
                        New projects will be added in addition to existing ones.
                </FileUploader>
            </div>
            <Button 
                label="Create Project" 
                @click="showCreateModal = true"
            />
        </div>
        <DataTable :value="projects" :loading="loading" :paginator="true" :rows="10" :rows-per-page-options="[5, 10, 20, 50]">
            <Column field="name" header="Name">
                <template #body="slotProps">
                    <InputText 
                        v-model="slotProps.data.name" 
                        class="w-full editable-input"
                        @input="markAsChanged(slotProps.data.id)"
                    />
                </template>
            </Column>
            <Column field="client" header="Client">
                <template #body="slotProps">
                    <InputText 
                        v-model="slotProps.data.client" 
                        class="w-full editable-input"
                        @input="markAsChanged(slotProps.data.id)"
                    />
                </template>
            </Column>
            <Column field="requiresNda" header="Requires NDA">
                <template #body="slotProps">
                    <Checkbox 
                        v-model="slotProps.data.requiresNda" 
                        :binary="true"
                        class="editable-checkbox"
                        @change="markAsChanged(slotProps.data.id)"
                    />
                </template>
            </Column>
            <Column field="minStudents" header="Min. Students">
                <template #body="slotProps">
                    <InputNumber 
                        v-model="slotProps.data.minStudents" 
                        :min="1"
                        class="w-full editable-input"
                        @input="markAsChanged(slotProps.data.id)"
                    />
                </template>
            </Column>
            <Column field="maxStudents" header="Max. Students">
                <template #body="slotProps">
                    <InputNumber 
                        v-model="slotProps.data.maxStudents" 
                        :min="1"
                        class="w-full editable-input"
                        @input="markAsChanged(slotProps.data.id)"
                    />
                </template>
            </Column>
            <Column field="minInstances" header="Min. Instances">
                <template #body="slotProps">
                    <InputNumber 
                        v-model="slotProps.data.minInstances" 
                        :min="0"
                        class="w-full editable-input"
                        @input="markAsChanged(slotProps.data.id)"
                    />
                </template>
            </Column>
            <Column field="maxInstances" header="Max. Instances">
                <template #body="slotProps">
                    <InputNumber 
                        v-model="slotProps.data.maxInstances" 
                        :min="1"
                        class="w-full editable-input"
                        @input="markAsChanged(slotProps.data.id)"
                    />
                </template>
            </Column>
            <Column field="id" header="Actions">
				<template #body="slotProps">
                    <Button severity="danger" class="i-mdi-delete" @click="deleteProject(slotProps.data.id)" />
                </template>
            </Column>
            <template #empty>
                <div class="text-center p-4 text-gray-500">
                  Projects list not uploaded, students will not be able to submit preferences until projects are added.
                </div>
            </template>
        </DataTable>

        <!-- Save Changes Button -->
        <div v-if="hasChanges" class="flex justify-end">
            <Button 
                label="Save Changes" 
                severity="success"
                :loading="saving"
                @click="saveChanges"
            />
        </div>

        <!-- Create Project Form Component -->
        <CreateProjectForm
            v-model:visible="showCreateModal"
            :class-id="classId"
            @created="(newProject) => setProjects([...projects, newProject])"
        />
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue';
import type { ProjectDto } from '../../dtos/project-dto';
import DataTable from 'primevue/datatable';
import Button from 'primevue/button';
import Divider from 'primevue/divider';
import Column from 'primevue/column';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import Checkbox from 'primevue/checkbox';
import TeacherNavBar from '../../components/TeacherNavBar.vue';
import FileUploader from '../../components/FileUploader.vue';
import type { FileUploadSelectEvent } from 'primevue/fileupload';
import ApiService from '../../services/ApiService';
import { useToast } from 'primevue/usetoast';
import { useRoute } from 'vue-router';
import CreateProjectForm from '../../components/CreateProjectForm.vue';

const projects = ref([] as ProjectDto[]);
const loading = ref(false);
const saving = ref(false);
const toast = useToast();
const route = useRoute();
const classId = route.params.classId as string;
const showCreateModal = ref(false);
const changedProjectIds = ref(new Set<number>());

// Store original project data for comparison
const originalProjects = ref(new Map<number, ProjectDto>());

const hasChanges = computed(() => changedProjectIds.value.size > 0);

onMounted(() => {
    getProjects();
});

const getProjects = async () => {
    try {
        loading.value = true;
        const projectData = await ApiService.get<ProjectDto[]>(`/projects?classId=${classId}`);
        setProjects(projectData);
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const setProjects = (data: ProjectDto[]) => {
    projects.value = data;
    // Store original data for comparison
    originalProjects.value.clear();
    data.forEach(project => {
        originalProjects.value.set(project.id, { ...project });
    });
    changedProjectIds.value.clear();
}

const markAsChanged = (projectId: number) => {
    changedProjectIds.value.add(projectId);
};

const saveChanges = async () => {
    if (changedProjectIds.value.size === 0) return;
    
    saving.value = true;
    try {
        const updatePromises = Array.from(changedProjectIds.value).map(async (projectId) => {
            const project = projects.value.find(p => p.id === projectId);
            if (project) {
                return await ApiService.put<ProjectDto>(`/projects/update/${projectId}`, project);
            }
        });
        
        await Promise.all(updatePromises);
        
        // Update original data after successful save
        changedProjectIds.value.forEach(projectId => {
            const project = projects.value.find(p => p.id === projectId);
            if (project) {
                originalProjects.value.set(projectId, { ...project });
            }
        });
        
        changedProjectIds.value.clear();
        toast.add({ severity: 'success', summary: 'Success', detail: 'Projects updated successfully', life: 5000 });
    } catch (error) {
        console.error(error);
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to update projects', life: 10000 });
    } finally {
        saving.value = false;
    }
};

const uploadProjects = async (event: FileUploadSelectEvent) => {
    if (!event.files) {
        console.error('selected no file, ignoring')
    }
    const selectedFile = event.files[0]
    const formData = new FormData()
    formData.append('file', selectedFile)
    try {
        const result = await ApiService.postRaw(`projects/upload?classId=${classId}`, formData)
        setProjects(result as ProjectDto[])
        if (result != undefined) {
            toast.add({ severity: 'success', summary: 'Success', detail: 'Projects uploaded', life: 5000 });
        } else {
            error();
        }
    } catch {
        error();
    }
}

const deleteProject = async (id: string) => {
    const newProjects = await ApiService.delete<ProjectDto[]>(`/projects/delete/${id}?classId=${classId}`);
    setProjects(newProjects);
}

const error = () => {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Projects file upload failed', life: 10000 })
}

</script>

<style scoped>
.editable-input,
.editable-input :deep(.p-inputnumber-input)
{
    border: none !important;
    background: transparent !important;
    box-shadow: none !important;
    padding: 0.25rem 0.5rem !important;
    border-radius: 4px !important;
    transition: all 0.2s ease !important;
}

.editable-input :deep(.p-inputnumber-input:hover),
input[type="text"].editable-input:hover  {
    background: rgba(0, 0, 0, 0.05);
    outline: 1px solid rgba(0, 0, 0, 0.1);
}

input[type="text"].editable-input:focus,
.editable-input :deep(.p-inputnumber-input:focus) {
    background: white;
    outline: 2px solid #3b82f6;
    box-shadow: 0 0 0 1px rgba(59, 130, 246, 0.1);
}

.editable-checkbox :deep(.p-checkbox-box) {
    border: 1px solid #d1d5db;
    transition: all 0.2s ease;
}

.editable-checkbox :deep(.p-checkbox-box:hover) {
    border-color: #3b82f6;
    background: rgba(59, 130, 246, 0.1);
}

.editable-checkbox :deep(.p-checkbox-box.p-focus) {
    border-color: #3b82f6;
    box-shadow: 0 0 0 1px rgba(59, 130, 246, 0.1);
}
</style>
