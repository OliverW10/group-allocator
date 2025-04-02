<template>
    <div class="flex flex-col gap-4 p-4 h-screen justify-center">
        <h1 class="heading text-center mb-4">Submit Preferences</h1>
        <Card class="min-w-3xl mx-auto">
            <template #content>
                <form class="flex flex-col gap-4" @submit.prevent="submitForm">
                    <div class="flex-auto">
                        <label for="studentEmail" class="font-bold block mb-2">Student Email</label>
                        <InputText id="studentEmail" :placeholder="authStore.userInfo?.email" disabled />
                    </div>
                    <div class="flex flex-row gap-2">
                        <Checkbox id="consent" v-model="form.willingToSignContract" :binary="true" />
                        <label for="consent" class="font-bold block mb-2">Has Provided Consent Form?</label>
                    </div>

                    <Divider />

                    <div class="flex-auto">
                        <label class="font-bold block mb-2">Available Preferences</label>

                        <OrderList v-if="form.preferences.length > 0" v-model="form.preferences" dragdrop data-key="id"
                            :list-style="{ 'height': '250px' }">
                            <template #option="{ option }">
                                {{ allAvailablePreferences.find(p => p.id === option)?.name }}
                            </template>
                        </OrderList>

                        <div class="p-3 flex justify-between">
                            <Button label="Add New" severity="secondary" text size="small" icon="i-mdi-plus"
                                @click="showAddPreferenceDialog = true" />
                            <Button label="Remove All" severity="danger" text size="small" icon="i-mdi-times"
                                @click="removeAll" />
                        </div>

                        <Card v-if="showAddPreferenceDialog && allAvailablePreferences.length > 0" class="p-4">
                            <template #header>
                                <h2>Add New Preference</h2>
                            </template>

                            <template #content>
                                <Listbox multiple :options="allAvailablePreferences" option-label="name"
                                    @update:model-value="addPreference" />
                            </template>
                        </Card>
                    </div>

                    <Button type="submit" variant="text" label="Next" />
                </form>
            </template>
        </Card>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import InputText from "primevue/inputtext";
import Checkbox from "primevue/checkbox";
import Button from "primevue/button";
import Card from "primevue/card";
import Divider from "primevue/divider";
import Listbox from "primevue/listbox";
import { OrderList } from "primevue";
import { useAuthStore } from "../store/auth";
import type { StudentPreferencesDto } from "../dtos/student-preferences-dto";
import type { ProjectDto } from "../dtos/project-dto";

const authStore = useAuthStore();

const form = ref({
    preferences: [],
    willingToSignContract: false,
    fileNames: [],
    fileBlobs: [],
    id: 0,
} as StudentPreferencesDto);

const allAvailablePreferences = ref([
    { id: 1, name: "Preference 1", description: "", requiresContract: false },
    { id: 2, name: "Preference 2", description: "", requiresContract: true },
    { id: 3, name: "Preference 3", description: "", requiresContract: false },
    { id: 4, name: "Preference 4", description: "", requiresContract: true },
    { id: 5, name: "Preference 5", description: "", requiresContract: false },
] as ProjectDto[]);


const showAddPreferenceDialog = ref(false);

const submitForm = () => {
    console.log("Form submitted", form.value);
};

const removeAll = () => {
    form.value.preferences = [];
};

const addPreference = (preference: ProjectDto[]) => {
    if (!form.value.preferences) {
        form.value.preferences = [];
    }

    form.value.preferences.push(preference[0].id);

    showAddPreferenceDialog.value = false;
};
</script>