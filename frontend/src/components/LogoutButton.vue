<template>
    <div class="flex items-center gap-2">
        <span>
            Logged in as: {{ authStore.userInfo?.email }}
        </span>
        <Button severity="danger" label="Logout" icon="i-mdi-logout" class="p-button-text" @click="logout" />
    </div>
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