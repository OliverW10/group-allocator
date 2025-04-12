<template>
    <AdminNavBar />
    <DataTable :value="projects" :loading="loading" :paginator="true" :rows="30" :rows-per-page-options="[30, 100]">
            <Column field="name" header="Name"></Column>
            <Column field="requiresContract" header="Requires Contract"></Column>
            <Column field="requiresNda" header="Requires Nda"></Column>
            <Column field="minStudents" header="Min Students"></Column>
            <Column field="maxStudents" header="maxStudents"></Column>
            <Column field="id" header="Actions">
                <template #body="slotProps">
                    <Button label="X" class="p-button-text" @click="remove(slotProps.data.id)" />
                </template> 
            </Column>
        </DataTable>
</template>
<script setup lang="ts">
import { ref } from 'vue';
import AdminNavBar from '../../components/AdminNavBar.vue';
import type { ProjectDto } from '../../dtos/project-dto';
import ApiService from '../../services/ApiService';

const projects = ref([] as Student[]);
const loading = ref(false);

const remove = (id: string) => {
    ApiService.delete<StudentDto>
}

</script>