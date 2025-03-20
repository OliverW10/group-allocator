<script setup lang="ts">
import Menubar from 'primevue/menubar';
import { ref } from 'vue';

const items = ref([
  {
    label: 'Home',
    icon: 'i-mdi-home',
    url: '/home'
  },
  {
    label: 'Account',
    icon: 'i-mdi-account',
    url: '/account'
  },
  {
    label: 'Preferences',
    icon: 'i-mdi-settings',
    url: '/student/preferences'
  },
]);
</script>

<template>
  <Menubar :model="items">
    <template #item="menuitem">
      <router-link :to="menuitem.item.url ?? '/'" class="flex items-center py-1 px-2 transition-all">
        <div :class="menuitem.item.icon" style="margin-right: 8px;"></div>
        <span>{{ menuitem.label }}</span>
      </router-link>
    </template>
  </Menubar>
</template>