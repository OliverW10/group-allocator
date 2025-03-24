<script setup lang="ts">
import { onMounted } from 'vue';
import { useAuthStore } from '../store/auth';

const authStore = useAuthStore();

onMounted(async () => {
    await fetch("https://localhost:7000/admin/status/", {
        credentials: "include"
    })
});
</script>

<template>
  <div>
    <h1 class="heading">Welcome</h1>
  </div>
  <p>
    Logged in as {{authStore.userInfo?.email}}
  </p>
</template>