<template>
    <div class="p-8">
        <div class="flex justify-between items-center mb-8">
            <div class="flex items-center gap-4">
                <Button @click="goBack" icon="pi pi-arrow-left" label="Back" class="p-button-text mr-4" />
                <h1 class="text-2xl font-bold">Upgrading Class: {{ classInfo?.name || 'Loading...' }}</h1>
            </div>
            <div class="flex items-center gap-4">
                <LogoutButton />
            </div>
        </div>
        <div id="checkout">
            <!-- Checkout will insert the payment form here -->
        </div>
    </div>
</template>
<script setup lang="ts">
import { nextTick, onMounted, ref } from 'vue';
import { loadStripe } from '@stripe/stripe-js/pure';
import { useRoute, useRouter } from 'vue-router';
import ApiService from '../../services/ApiService';
import { ClassInfoDto } from '../../dtos/class-info-dto';
import LogoutButton from '../../components/LogoutButton.vue';
import Button from 'primevue/button';

const stripe_promise = loadStripe("pk_test_51Ra7ErGfx18ZU63nHXhCGFy5Ji3jORzOLasQbhWyWxKTAK0org9f2GxLivakJhB0xy1yemiK3DeqZJ3XyfBvNjMA00uBryjbe0");
const route = useRoute();
const router = useRouter();
const classId = route.params.classId as string;
const classInfo = ref<ClassInfoDto | null>(null);

function goBack() {
    router.back();
}

onMounted(async () => {
    await nextTick();
    await fetchClassInfo();
    await initialize();
});

// Fetch class information
async function fetchClassInfo() {
    try {
        classInfo.value = await ApiService.get<ClassInfoDto>(`/class/code/${classId}`);
    } catch (error) {
        console.error('Failed to fetch class info:', error);
    }
}

// Create a Checkout Session
async function initialize() {
    const stripe = await stripe_promise;
    if (stripe == undefined) {
        console.error("Could not load stripe")
        return;
    }
    const fetchClientSecret = async () => {
        const response = await fetch(`/create-checkout-session?classId=${classId}`, {
            method: "POST",
        });
        const { clientSecret } = await response.json();
        return clientSecret;
    };

    const checkout = await stripe.initEmbeddedCheckout({
        fetchClientSecret,
    });

    // Mount Checkout
    checkout.mount('#checkout');
}
</script>

