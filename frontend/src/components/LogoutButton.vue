
<template>
Logged in as: {{ authStore.userInfo?.email }}
            <Button 
                label="Logout" 
                icon="pi pi-sign-out" 
                class="p-button-text"
                @click="logout"
            />
</template>

<script setup lang="ts">
import Button from 'primevue/button'

import ApiService from '../services/ApiService';
import { useAuthStore } from '../store/auth'
import { useRouter } from 'vue-router'
const authStore = useAuthStore();
const router = useRouter()

const logout = () => {
    ApiService.get("/auth/logout");
    authStore.userInfo = null;
    router.push('/')
}
</script>