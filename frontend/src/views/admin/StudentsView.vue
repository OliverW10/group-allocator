<template>
    <AdminNavBar />
    <div class="px-4 py-2 mt-4 flex flex-col gap-4">
        <h1 class="heading">Students</h1>
        <Divider style="margin: 0;" />
        <FileUploader @projects-changed="uploadStudents">
            <p>
                Please upload a text (/csv) file with emails on each line.
                <br />
                Note that students will not appear on this page until they have submitted their preferences.
            </p>
        </FileUploader>
        <DataTable :value="students" :loading="loading" :paginator="true" :rows="10" :rows-per-page-options="[5, 10, 20, 50]">
            <Column field="name" header="Name"></Column>
            <Column field="email" header="Email"></Column>
            <Column field="willSignContract" header="NDA?">
                <template #body="slotProps">
                    {{ slotProps.data.willSignContract ? '✔️' : '❌' }}
                </template>
            </Column>
            <Column field="orderedPreferences" header="Preferences">
                <template #body="slotProps">
                    {{slotProps.data.orderedPreferences.map((projId: number) => projects.find((proj: ProjectDto) =>
                        projId == proj.id)?.name).join(', ')}}
                </template>
            </Column>
            <Column field="id" header="Actions">
                <template #body="slotProps">
                    <Button severity="danger" class="i-mdi-delete" @click="remove(slotProps.data.id)" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
<script setup lang="ts">
import { onMounted, ref } from 'vue';
import AdminNavBar from '../../components/AdminNavBar.vue';
import ApiService from '../../services/ApiService';
import { StudentSubmissionDto } from '../../dtos/student-dto';
import DataTable from 'primevue/datatable';
import Button from 'primevue/button';
import Column from 'primevue/column';
import Divider from 'primevue/divider';
import { useToast, type FileUploadSelectEvent } from 'primevue';
import { ProjectDto } from '../../dtos/project-dto';
import FileUploader from '../../components/FileUploader.vue';

// TODO: refactor 3 table views into generic component

const students = ref([] as StudentSubmissionDto[]);
const projects = ref([] as ProjectDto[])
const loading = ref(false);
const toast = useToast();

onMounted(async () => {
    try {
        loading.value = true;
        students.value = await ApiService.get<StudentSubmissionDto[]>("/students")
        projects.value = await ApiService.get<ProjectDto[]>("/projects")
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
});

const setStudents = (data: StudentSubmissionDto[]) => {
    students.value = data;
}

const remove = async (id: string) => {
    const newProjects = await ApiService.delete<StudentSubmissionDto[]>(`/students/${id}`);
    setStudents(newProjects);
}

const uploadStudents = async (event: FileUploadSelectEvent) => {
    if (!event.files) {
        console.error('selected no file, ignoring')
    }
    const selectedFile = event.files[0]
    const formData = new FormData()
    formData.append('file', selectedFile)
    try {
        const result = await ApiService.postRaw('students/whitelist', formData) // , 'multipart/form-data'
        setStudents(result as StudentSubmissionDto[])
        if (result != undefined) {
            toast.add({ severity: 'success', summary: 'Success', detail: 'Students added to whitelist', life: 5000 });
        } else {
            error();
        }
    } catch {
        error();
    }
}

const error = () => {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Students whitelist file upload failed', life: 10000 })
}
</script>
