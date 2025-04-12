<template>
    <div class="flex flex-col gap-4 p-4 h-screen justify-center">
        <h1 class="heading text-center mb-4">Submit Preferences</h1>
        <Card class="min-w-3xl mx-auto">
            <template #content>
            <div class="p-4">
                <h2 class="text-xl mb-3">Select Your Project Preferences</h2>

                <PickList
                v-model:source="availableProjects"
                v-model:target="preferredProjects"
                listStyle="height:300px"
                dataKey="id"
                dragdrop
                >
                    <template #sourceheader>Available Projects</template>
                    <template #targetheader>Preferred Projects (ordered)</template>

                    <template #item="slotProps">
                        <div class="flex flex-col">
                        <span class="font-medium">{{ slotProps.item.name }}</span>
                        <small class="text-gray-500">Client: {{ slotProps.item.client }}</small>
                        </div>
                    </template>
                </PickList>
                <ToggleSwitch v-model="checked" />

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
import type { StudentPreferencesDto } from "../dtos/student-preferences-dto";
import { ProjectDto } from "../dtos/project-dto";
import LogoutButton from "../components/LogoutButton.vue";
import { onMounted, ref } from 'vue'
import PickList from 'primevue/picklist'
import ApiService from "../services/ApiService";


// Sample data — in a real app, fetch from API
const availableProjects = ref([] as ProjectDto[])
const preferredProjects = ref([] as ProjectDto[])

onMounted(() => {
    availableProjects.value = ApiService.get<ProjectDto[]>("/projects/get")
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
};
</script>