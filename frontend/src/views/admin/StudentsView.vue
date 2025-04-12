<template>
    <AdminNavBar />
    <h1 class="heading">Students</h1>
    <Divider />
    <DataTable :value="students" :loading="loading" :paginator="true" :rows="30" :rows-per-page-options="[30, 100]">
        <Column field="email" header="Email"></Column>
        <Column field="willSignContract" header="Contract?"></Column>
        <Column field="orderedPreferences" header="Preferences"></Column>
        <Column field="id" header="Actions">
            <template #body="slotProps">
                <Button label="X" class="p-button-text" @click="remove(slotProps.data.id)" />
            </template> 
        </Column>
    </DataTable>
    <FileUpload mode="basic" name="file" :auto="false" accept=".csv,.txt" choose-label="Upload Students File" @select="onSelect" />

</template>
<script setup lang="ts">
import { onMounted, ref } from 'vue';
import AdminNavBar from '../../components/AdminNavBar.vue';
import ApiService from '../../services/ApiService';
import { StudentDto } from '../../dtos/student-dto';
import DataTable from 'primevue/datatable';
import Button from 'primevue/button';
import Column from 'primevue/column';
import Divider from 'primevue/divider';
import type { FileUploadSelectEvent } from 'primevue';

// TODO: refactor 3 table views into generic component

const students = ref([] as StudentDto[]);
const loading = ref(false);
const filterList = ref(undefined as string[] | undefined)

onMounted(() => {
    getStudents();
});

const getStudents = async () => {
    try {
        loading.value = true;
        setStudents(await ApiService.get<StudentDto[]>("/students"))
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const setStudents = (data: StudentDto[]) => {
    students.value = data;
}

const remove = async (id: string) => {
    const newProjects = await ApiService.delete<StudentDto[]>(`/students/${id}`);
    setStudents(newProjects);
}

const onSelect = async (event: FileUploadSelectEvent) => {
    if (!event.files) {
        console.error('selected no file, ignoring')
    }
    const selectedFile = event.files[0]
    const reader = new FileReader()
    reader.onload = (e) => {
        const contents = e.target?.result as string; // :P
        filterList.value = contents.split("\n")
    }
    reader.readAsText(selectedFile)
}
</script>