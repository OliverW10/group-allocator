<script setup lang="ts">
import router from '../router';
import { useAuthStore } from '../store/auth';

const authStore = useAuthStore();
const isLoggedIn = authStore.userInfo;
if (isLoggedIn) {
  const isAdmin = authStore.userInfo?.isAdmin ?? false;
  if (isAdmin) {
    router.push({ name: 'Dashboard' });
  } else {
    router.push({ name: 'Form' });
  }
}
</script>

<template>
  <div class="flex flex-col justify-center items-center">
    <h1 class="heading py-4">My Account</h1>
    <div>
      <button class="flex items-center w-max p-3 rounded-md">
        Sign in with Microsoft
        <i class="i-logos-microsoft-icon ml-2"></i>
      </button>
    </div>
    <div v-if="import.meta.env.DEV">
      <button class="flex items-center w-max p-3 rounded-md">
        Development Test Login
        <i class="i-logos-microsoft-icon ml-2"></i>
      </button>
    </div>
  </div>
</template>