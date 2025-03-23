<template>
    <div>
        <h1 class="heading">Student Preference Form</h1>
        <Card>
            <template #content>
                <form @submit.prevent="submitForm" class="flex flex-col gap-4">
                    <div class="flex-auto">
                        <label for="firstName" class="font-bold block mb-2">First Name</label>
                        <InputText id="firstName" v-model="form.firstName" placeholder="Enter First Name" />
                    </div>
                    <div class="flex-auto">
                        <label for="lastName" class="font-bold block mb-2">Last Name</label>
                        <InputText id="lastName" v-model="form.lastName" placeholder="Enter Last Name" />
                    </div>
                    <div class="flex-auto">
                        <label for="studentNumber" class="font-bold block mb-2">Student Number</label>
                        <InputText id="studentNumber" v-model="form.studentNumber" placeholder="Enter Student Number" />
                    </div>
                    <div class="flex-auto">
                        <label for="studentEmail" class="font-bold block mb-2">Student Email</label>
                        <InputText id="studentEmail" v-model="form.email"
                            placeholder="example.email@student.uts.edu.au" />
                    </div>
                    <div class="flex-auto">
                        <Checkbox id="consent" v-model="form.hasProvidedConsentForm" :binary="true" />
                        <label for="consent" class="font-bold block mb-2">Has Provided Consent Form?</label>
                    </div>
                       
                    <Divider />

                    <div class="flex-auto">
                        <label class="font-bold block mb-2">Available Preferences</label>

                        <OrderList v-if="form.preferences.length > 0"            
                            v-model="form.preferences"
                            dragdrop
                            data-key="id"
                            :list-style="{'height': '250px'}"
                            >

                            <template #option="{ option }">
                                {{ option }}
                            </template>
                        </OrderList>

                        <div class="p-3 flex justify-between">
                            <Button label="Add New" severity="secondary" text size="small" icon="pi pi-plus" @click="showAddPreferenceDialog = true" />
                            <Button label="Remove All" severity="danger" text size="small" icon="pi pi-times" @click="removeAll" />
                        </div>

                        <Card v-if="showAddPreferenceDialog && allAvailablePreferences.length > 0" class="p-4">
                            <template #header>
                                <h2>Add New Preference</h2>
                            </template>

                            <template #content>
                                <Listbox @update:model-value="addPreference" multiple :options="allAvailablePreferences" optionLabel="name" />
                            </template>
                        </Card>
                    </div>

                    <Button type="submit" variant="text" label="Next" class="p-mt-3" />
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
import type StudentPreferencesFormDto from "../models/StudentPreferencesFormDto";
import type GroupAllocationOption from "../models/GroupAllocationOption";
import { OrderList } from "primevue";

const form = ref({
    firstName: "",
    lastName: "",
    email: "",
    studentNumber: "",
    hasProvidedConsentForm: false,
    preferences: [],
} as StudentPreferencesFormDto);

const allAvailablePreferences = ref([
    { id: 1, name: "Preference 1" },
    { id: 2, name: "Preference 2" },
    { id: 3, name: "Preference 3" },
    { id: 4, name: "Preference 4" },
    { id: 5, name: "Preference 5" },
] as GroupAllocationOption[]);


const showAddPreferenceDialog = ref(false);

const submitForm = () => {
    console.log("Form submitted", form.value);
};

const removeAll = () => {
    form.value.preferences = [];
};

const addPreference = (preference: GroupAllocationOption) => {
    console.log("Adding preference", preference);
    if (!form.value.preferences) {
        form.value.preferences = [];
    }

    form.value.preferences.push(preference);

    showAddPreferenceDialog.value = false;
    console.log("Preferences", form.value.preferences);
};
</script>