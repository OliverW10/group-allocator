<template>
    <div class="flex flex-col gap-4 p-4 h-screen justify-center">
        <h1 class="heading text-center mb-4">Submit Preferences</h1>
        <Card class="min-w-5xl mx-auto">
            <template #content>
            <div class="p-4">
                <h2 class="text-xl mb-3">Select Your Project Preferences</h2>

                <PickList
                    v-model:model-value="projects"
                    list-style="min-height: 500px"
                    data-key="id"
                    dragdrop
                >
                    <template #sourceheader><b class="text-lg">Available Projects</b></template>
                    <template #targetheader><b class="text-lg">Ordered Preferences</b></template>

                    <template #option="{ option  }">
                        {{ option.name }}
                    </template>
                </PickList>
                <label for="switch1">Are you willing to sign a contract to work on a project</label>
                <ToggleSwitch v-model="student.willSignContract" input-id="switch1" />

                <FileUpload name="demo[]" url="/api/upload" :multiple="true" :max-file-size="10000000" @upload="onUpload($event)">
                    <template #empty>
                        <span>Drag and drop files to here to upload.</span>
                    </template>
                </FileUpload>

                <Button label="Save Preferences" class="mt-4" icon="pi pi-check" @click="submitForm" />
            </div>
            </template>
        </Card>
        <LogoutButton />
    </div>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import Card from "primevue/card";
import ToggleSwitch from "primevue/toggleswitch";
import FileUpload, { type FileUploadUploadEvent } from "primevue/fileupload";
import { ProjectDto } from "../dtos/project-dto";
import LogoutButton from "../components/LogoutButton.vue";
import { onMounted, ref } from 'vue'
import PickList from 'primevue/picklist'
import ApiService from "../services/ApiService";
import { StudentDto } from "../dtos/student-dto";
import { useToast } from "primevue/usetoast";
import { useAuthStore } from '../store/auth'

const authStore = useAuthStore();
const toast = useToast();

const DEFAULT_STUDENT: StudentDto = {
    name: "",
    email: "",
    id: -1,
    orderedPreferences: [],
    fileNames: [],
    willSignContract: false,
}
const student = ref(DEFAULT_STUDENT)
const projects = ref([[], []] as ProjectDto[][]);

onMounted(async () => {
    const maybeStudent = await ApiService.get<StudentDto | undefined>("/students/me")
    if (maybeStudent){
        student.value = maybeStudent
        toast.add({ severity: 'success', summary: 'Success', detail: 'Loaded previous submission', life: 3000 });
    }
    const allProjects = await ApiService.get<ProjectDto[]>("/projects")
    if (allProjects.length == 0){
        console.warn("no projects")
        toast.add({ severity: 'warn', detail: 'No projects found in system'})
        return
    }
    projects.value = [allProjects, []]
    for (const selectedProj of student.value.orderedPreferences){
        const relevant = projects.value[0].filter(x => x.id == selectedProj)[0]
        projects.value[1].push(relevant)
        projects.value[0] = projects.value[0].filter(x => x.id != relevant.id)
    }
})

const submitForm = () => {
    const submitModel: StudentDto = {
        name: student.value.name,
        id: student.value.id,
        email: authStore.userInfo?.email ?? student.value.email,
        fileNames: student.value.fileNames,
        orderedPreferences: projects.value[1].map(p => p.id), // should this be id's or names?
        willSignContract: student.value.willSignContract
    }
    ApiService.post("/students/me", submitModel);
    toast.add({ severity: 'success', summary: 'Success', detail: 'Submitted preferences', life: 3000 });
};

const onUpload = (event: FileUploadUploadEvent) => {
    // todo
    // probably don't want a seperate upload button for the files
    console.log(event)
}

</script>