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
                <ToggleSwitch input-id="switch1" v-model="student.willSignContract" />

                <FileUpload name="demo[]" url="/api/upload" @upload="onAdvancedUpload($event)" :multiple="true" accept="image/*" :maxFileSize="10000000">
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
import FileUpload from "primevue/fileupload";
import type { StudentPreferencesDto } from "../dtos/student-preferences-dto";
import { ProjectDto } from "../dtos/project-dto";
import LogoutButton from "../components/LogoutButton.vue";
import { onMounted, ref } from 'vue'
import PickList from 'primevue/picklist'
import ApiService from "../services/ApiService";
import type { StudentDto } from "../dtos/student-dto";
import { useToast } from "primevue/usetoast";

const toast = useToast();

const DEFAULT_STUDENT: StudentDto = {
    email: "loading",
    id: -1,
    orderedPreferences: [],
    fileNames: [],
    willSignContract: false,
}
const student = ref(DEFAULT_STUDENT)
const projects = ref([[], []] as ProjectDto[][]);

onMounted(async () => {
    const maybeStudent = await ApiService.get<StudentDto | undefined>("/students/mine")
    if (maybeStudent){
        student.value = maybeStudent
        toast.add({ severity: 'success', summary: 'Success', detail: 'Loaded previous submission', life: 3000 });
    }
    const allProjects = await ApiService.get<ProjectDto[]>("/projects/get")
    projects.value = [allProjects, []]
    if (projects.value.length == 0){
        toast.add({ severity: 'warn', detail: 'No projects found in system'})
    }
})

const form = ref({
    preferences: [],
    willingToSignContract: false,
    fileNames: [],
    fileBlobs: [],
    id: 0,
} as StudentPreferencesDto);

const submitForm = () => {
    console.log("Form submitted", form.value);
    toast.add({ severity: 'success', summary: 'Success', detail: 'Submitted preferences', life: 3000 });
};
</script>