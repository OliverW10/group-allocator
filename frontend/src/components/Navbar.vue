<script setup lang="ts">
import Menubar from 'primevue/menubar';
import { ref } from 'vue';

const items = ref([
  {
    label: 'Home',
    icon: 'i-mdi-home',
  },
]);
</script>

<template>
  <Menubar :model="items" />
</template>