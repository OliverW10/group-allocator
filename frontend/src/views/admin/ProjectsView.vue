<template>
    <div>
        <AdminNavBar />
        <h1 class="heading">Projects</h1>
        <Divider />

        <FileUploader @projects-changed="uploadProjects">
            <p>
                Please upload a csv file with the following format and no header
                <br />
                <code>project name, client, min_students, max_students, requires_nda</code>
                <br />
                New projects will be added in addition to existing ones.
            </p>
        </FileUploader>

        <DataTable :value="projects" :loading="loading" :paginator="true" :rows="30" :rows-per-page-options="[30, 100]">
            <Column field="name" header="Name"></Column>
            <Column field="requiresContract" header="Requires Contract"></Column>
            <Column field="requiresNda" header="Requires Nda"></Column>
            <Column field="minStudents" header="Min Students"></Column>
            <Column field="maxStudents" header="maxStudents"></Column>
            <Column field="id" header="Actions">
                <template #body="slotProps">
                    <Button label="X" class="p-button-text" @click="deleteProject(slotProps.data.id)" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import type { ProjectDto } from '../../dtos/project-dto';
import DataTable from 'primevue/datatable';
import Button from 'primevue/button';
import Divider from 'primevue/divider';
import Column from 'primevue/column';
import ProjectService from '../../services/ProjectService';
import AdminNavBar from '../../components/AdminNavBar.vue';
import FileUploader from '../../components/FileUploader.vue';
import type { FileUploadSelectEvent } from 'primevue/fileupload';
import ApiService from '../../services/ApiService';
import { useToast } from 'primevue/usetoast';

const projects = ref([] as ProjectDto[]);
const loading = ref(false);
const toast = useToast();

onMounted(() => {
    getProjects();
});

const getProjects = async () => {
    try {
        loading.value = true;
        setProjects(await ProjectService.getProjects())
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const setProjects = (data: ProjectDto[]) => {
    projects.value = data;
}

const uploadProjects = async (event: FileUploadSelectEvent) => {
    if (!event.files) {
        console.error('selected no file, ignoring')
    }
    const selectedFile = event.files[0]
    const formData = new FormData()
    formData.append('file', selectedFile)
    try {
        const result = await ApiService.postRaw('projects/upload', formData) // , 'multipart/form-data'
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
    const newProjects = await ProjectService.deleteProject(id);
    setProjects(newProjects);
}

const error = () => {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Projects file upload failed', life: 10000 })
}

</script>