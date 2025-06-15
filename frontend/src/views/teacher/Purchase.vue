<template>
    <div id="checkout">
        <!-- Checkout will insert the payment form here -->
    </div>
</template>
<script setup lang="ts">
import { nextTick, onMounted } from 'vue';
import { loadStripe } from '@stripe/stripe-js/pure';

const stripe = await loadStripe("pk_test_51Ra7ErGfx18ZU63nHXhCGFy5Ji3jORzOLasQbhWyWxKTAK0org9f2GxLivakJhB0xy1yemiK3DeqZJ3XyfBvNjMA00uBryjbe0");

onMounted(async () => {
    await nextTick();
    initialize();
});

// Create a Checkout Session
async function initialize() {
    if (stripe == undefined) {
        console.error("Could not load stripe")
        return;
    }
    const fetchClientSecret = async () => {
        const response = await fetch("/create-checkout-session", {
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

