<template>
    <Menubar :model="items" />
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import Menubar from 'primevue/menubar'

const router = useRouter()

const items = ref([
{
    label: 'Projects',
    icon: 'pi pi-briefcase',
    command: () => router.push('/admin/projects')
},
{
    label: 'Students',
    icon: 'pi pi-users',
    command: () => router.push('/admin/students')
},
{
    label: 'Solver',
    icon: 'pi pi-cog',
    command: () => router.push('/admin/solver')
}
])
</script>
