<template>
    <div>
        <h1 class="heading">Projects</h1>
        <Divider />
        <DataTable :value="projects" :loading="loading" :paginator="true" :rows="10" :rows-per-page-options="[5, 10, 20]">
            <Column field="name" header="Name"></Column>
            <Column field="requiresContract" header="Requires Contract"></Column>
            <Column field="requiresNda" header="Requires Nda"></Column>
            <Column field="minStudents" header="Min Students"></Column>
            <Column field="maxStudents" header="maxStudents"></Column>
            <Column field="id" header="Actions">
                <template #body="slotProps">
                    <Button label="View" @click="openProjectDetails(slotProps.data.id)" class="p-button-text" />
                </template> 
            </Column>
        </DataTable>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import type { ProjectDto } from '../dtos/project-dto';
import DataTable from 'primevue/datatable';
import Button from 'primevue/button';
import Divider from 'primevue/divider';
import Column from 'primevue/column';
import ProjectService from '../services/ProjectService';
import { useRouter } from 'vue-router';

const projects = ref([] as ProjectDto[]);

const loading = ref(false);

const router = useRouter();

onMounted(() => {
    getProjects();
});

const getProjects = async () => {
    try {
        loading.value = true;
        const response = await ProjectService.getProjects();
        projects.value = response;
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const openProjectDetails = (projectId: string) => {
    const route = `/projects/${projectId}`;
    router.push(route);
};

</script>