<template>
    <div class="p-8 min-h-screen bg-gray-50">
        <div class="max-w-2xl mx-auto bg-white rounded-lg shadow-lg p-8">
            <div class="flex justify-between items-center mb-8">
                <div class="flex items-center gap-4">
                    <Button icon="pi pi-arrow-left" label="Back" class="p-button-text mr-4" @click="goBack" />
                    <h1 class="text-2xl font-bold">Payment Finished for class: <span class="text-primary">{{ classInfo?.name || 'Loading...' }}</span></h1>
                </div>
                <div class="flex items-center gap-4">
                    <LogoutButton />
                </div>
            </div>
            <div class="space-y-6">
                <div v-if="success === false" class="bg-red-100 text-red-700 rounded p-4">
                    <p>Received payment cancelled from Stripe</p>
                </div>
                <div v-if="success === true" class="bg-green-100 text-green-700 rounded p-4">
                    <p>Payment successful from Stripe</p>
                </div>
                <div v-if="recieved === true" class="bg-blue-50 border border-blue-200 rounded p-6 text-center">
                    <h2 class="text-xl font-semibold mb-2 text-blue-800">Payment verified by Group Allocator</h2>
                    <p class="mb-4">Class is now upgraded to a paid plan</p>
                    <Button label="Return to classes page" class="p-button-success" @click="goToClass" />
                </div>
                <div v-else-if="recieved === false" class="bg-yellow-100 text-yellow-800 rounded p-4 text-center">
                    <p class="mb-2">Payment was not verified by Group Allocator</p>
                    <Button label="Retry Payment" class="p-button-warning" @click="validatePayment" />
                </div>
                <div v-if="recieved === null" class="flex items-center justify-center gap-2 text-gray-500">
                    <span class="pi pi-spin pi-spinner"></span>
                    <span>Validating payment...</span>
                </div>
            </div>
        </div>
    </div>
</template>
<script setup lang="ts">
import { nextTick, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ApiService from '../../services/ApiService';
import { ClassInfoDto } from '../../dtos/class-info-dto';
import LogoutButton from '../../components/LogoutButton.vue';
import Button from 'primevue/button';

const route = useRoute();
const router = useRouter();
const classId = route.params.classId as string;
const classInfo = ref<ClassInfoDto | null>(null);
const success = ref<boolean | null>(null);
const recieved = ref<boolean | null>(null);

function goBack() {
    router.back();
}

onMounted(async () => {
    await nextTick();
    await fetchClassInfo();
    success.value = route.query.success === 'true';
    if (success.value) {
        recieved.value = await validatePayment();
    }
});

// Fetch class information
async function fetchClassInfo() {
    try {
        classInfo.value = await ApiService.get<ClassInfoDto>(`/class/code/${classId}`);
    } catch (error) {
        console.error('Failed to fetch class info:', error);
    }
}

function goToClass() {
    router.push(`/teacher`);
}

// Create a Checkout Session
async function validatePayment() {
    return await ApiService.get<boolean>(`/payment/verify-payment?classId=${classId}&sessionId=${route.query.session_id}`);
}
</script>

