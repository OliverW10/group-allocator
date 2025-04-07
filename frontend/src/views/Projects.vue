<template>
    <div>
        <h1 class="heading">Projects</h1>
        <Divider />

        <button @click="showModal = true">Upload Project</button>

        <ProjectUploadForm
            v-if="showModal"
            :show-modal="showModal"
            @close="showModal = false"
            @upload="handleProjectUpload"
        />

        <DataTable :value="projects" :loading="loading" :paginator="true" :rows="10" :rows-per-page-options="[5, 10, 20]">
            <Column field="name" header="Name"></Column>
            <Column field="requiresContract" header="Requires Contract"></Column>
            <Column field="requiresNda" header="Requires Nda"></Column>
            <Column field="minStudents" header="Min Students"></Column>
            <Column field="maxStudents" header="maxStudents"></Column>
            <Column field="id" header="Actions">
                <template #body="slotProps">
                    <Button label="View" class="p-button-text" @click="openProjectDetails(slotProps.data.id)" />
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
import ProjectUploadForm from '../components/UploadProjectsComponent.vue';
import { useRouter } from 'vue-router';

const projects = ref([] as ProjectDto[]);

const loading = ref(false);

const showModal = ref(false);

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

const handleProjectUpload = (formData: File) => {

    if (formData && formData.type === 'text/csv') {
        //parseCSV(formData);
    } else {
        alert('Please upload a CSV file');
    }
    console.log("Project uploaded:", formData);
};

// const parseCSV = (file: File) => {
//       const reader = new FileReader();

//       reader.onload = (e) => {
//         const csv = e.target?.result;

//         if (!csv) {
//             console.error("No CSV content found.");
//             return;
//         }

//         const rows = csv.split('\n');
//         const headers = rows[0].split(',');

//         // Split CSV content by lines
//         const rows = csv.split('\n');

//         // Assuming the first row contains headers
//         const headers = rows[0].split(',');

//         // Process each row and convert it into a DTO
//         const data = rows.slice(1).map((row) => {
//           const values = row.split(',');

//           // Map each row into a DTO using headers as the keys
//           let dto = {};
//           headers.forEach((header, index) => {
//             dto[header.trim()] = values[index]?.trim();  // Map header to value
//           });

//           return dto;
//         });

//         this.data = data;
//       };

//       reader.readAsText(file);  // Read the CSV file as text
//     }

const openProjectDetails = (projectId: string) => {
    const route = `/projects/${projectId}`;
    router.push(route);
};

</script>