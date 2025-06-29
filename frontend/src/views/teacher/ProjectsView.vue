<template>
    <AdminNavBar :classId="classId" />
    <div class="px-4 py-2 mt-4 flex flex-col gap-4">
        <h1 class="heading">Projects</h1>
        <Divider style="margin: 0;" />
        <FileUploader label="Add Projects" @projects-changed="uploadProjects">
            <p>
                Please upload a csv file with the following format and no header
                <br />
                <code>project_name, client, min_students, max_students, requires_nda, min_instances, max_instances</code>
                <br />
                New projects will be added in addition to existing ones.<br>
                To modify a project, delete it from the table and upload a file with just the new values for that project.
            </p>
        </FileUploader>
        <DataTable :value="projects" :loading="loading" :paginator="true" :rows="10" :rows-per-page-options="[5, 10, 20, 50]">
            <Column field="name" header="Name"></Column>
            <Column field="requiresNda" header="Requires NDA">
                <template #body="slotProps">
                    {{slotProps.data.requiresNda ? '✔️' : '❌'}}
                </template>
            </Column>
            <Column field="minStudents" header="Min. Students"></Column>
            <Column field="maxStudents" header="Max. Students"></Column>
            <Column field="minInstances" header="Min. Instances"></Column>
            <Column field="maxInstances" header="Max. Instances"></Column>
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
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import type { ProjectDto } from '../../dtos/project-dto';
import DataTable from 'primevue/datatable';
import Button from 'primevue/button';
import Divider from 'primevue/divider';
import Column from 'primevue/column';
import AdminNavBar from '../../components/TeacherNavBar.vue';
import FileUploader from '../../components/FileUploader.vue';
import type { FileUploadSelectEvent } from 'primevue/fileupload';
import ApiService from '../../services/ApiService';
import { useToast } from 'primevue/usetoast';
import { useRoute } from 'vue-router';

const projects = ref([] as ProjectDto[]);
const loading = ref(false);
const toast = useToast();
const route = useRoute();
const classId = route.params.classId as string;

onMounted(() => {
    getProjects();
});

const getProjects = async () => {
    try {
        loading.value = true;
        setProjects(await ApiService.get<ProjectDto[]>(`/projects?classId=${classId}`))
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
