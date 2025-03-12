<script setup lang="ts">
import { useAuthStore } from '../store/auth';

const authStore = useAuthStore();
</script>

<template>
  <div class="flex flex-col justify-center items-center">
    <h1 class="heading py-4">My Account</h1>
    <div v-if="authStore.token === null">
      <button class="flex items-center w-max p-3 rounded-md">
        Sign in with Microsoft
        <i class="i-logos-microsoft-icon ml-2"></i>
      </button>
    </div>
    <div v-else class="flex flex-col items-center">
      <p class="text-lg pb-2">You're signed in as {{ authStore.userInfo?.givenName }} ({{ authStore.userInfo?.email }}).</p>
      <button class="flex items-center w-max p-3 rounded-md">
        Sign out
      </button>
    </div>
  </div>
</template>