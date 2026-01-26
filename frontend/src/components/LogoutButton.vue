<template>
    <div class="flex items-center bg-gray-50 dark:bg-gray-700 border rounded-lg px-2 py-1 sm:px-3 sm:py-2 border-gray-200 dark:border-gray-600">
        <span class="sm:inline text-gray-700 dark:text-gray-50">
            Logged in as: {{ authStore.userInfo?.email.split('@')[0] }}
        </span>
        <span class="sm:hidden text-gray-700 dark:text-gray-50 text-sm">
            {{ authStore.userInfo?.email.split('@')[0] }}
        </span>
        <Button severity="danger" label="Logout" icon="i-mdi-logout" class="p-button-text text-sm sm:text-base" :loading="loading" @click="logout" />
    </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button'

import ApiService from '../services/ApiService';
import { useAuthStore } from '../store/auth'
import { useRouter } from 'vue-router'
import { onBeforeMount, ref } from 'vue';
import { UserInfoDto } from '../dtos/user-info-dto';
const authStore = useAuthStore();
const router = useRouter()

const loading = ref(false)

onBeforeMount(async () => {
    const userInfo = await ApiService.get<UserInfoDto>("/auth/me");
    if (userInfo) {
        authStore.userInfo = userInfo;
    }
})

const logout = async () => {
    loading.value = true
    await ApiService.get("/auth/logout");
    authStore.userInfo = null;
    router.push('/')
}
</script>
