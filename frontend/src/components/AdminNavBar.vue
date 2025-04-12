<template>
    <Menubar :model="items">
        <template #end>
            <LogoutButton />
        </template>
    </Menubar>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import LogoutButton from '../components/LogoutButton.vue'
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
