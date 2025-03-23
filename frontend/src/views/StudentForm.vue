<script setup lang="ts">
import { useAuthStore } from '../store/auth';

const authStore = useAuthStore();
</script>

<template>
  <div>
    <h1 class="heading">Welcome</h1>
  </div>
  <p>
    Logged in as {{authStore.userInfo?.email}}
  </p>
</template>