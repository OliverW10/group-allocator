<template>
    <div>
        <AdminNavBar />
        <h1 class="heading">Projects</h1>
        <Divider />

        <FileUploader @projects-changed="setProjects"></FileUploader>

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

const projects = ref([] as ProjectDto[]);
const loading = ref(false);

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

const deleteProject = async (id: string) => {
    const newProjects = await ProjectService.deleteProject(id);
    setProjects(newProjects);
}

</script>