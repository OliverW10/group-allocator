<template>
    <Menubar :model="items">
        <template #start>
            <Button label="Return to Classes" icon="i-mdi-logout" class="p-button-text" @click="returnToDashboard" />
        </template>
        <template #end>
            <div class="flex items-center gap-2">
                <LogoutButton />
            </div>
        </template>
    </Menubar>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import LogoutButton from '../components/LogoutButton.vue'
import Menubar from 'primevue/menubar'
import Button from 'primevue/button'

const router = useRouter()
const route = useRoute()

// Accept classId as a prop, fallback to route param
const props = defineProps<{ classId?: string | number, disabled?: boolean }>()
const classId = computed(() => props.classId ?? route.params.classId)

const returnToDashboard = () => {
    router.push(`/teacher`)
}

const items = ref([
{
    label: 'Projects',
    icon: 'i-mdi-briefcase',
    command: () => router.push(`/teacher/${classId.value}/projects`)
},
{
    label: 'Students',
    icon: 'i-mdi-users',
    command: () => router.push(`/teacher/${classId.value}/students`)
},
{
    label: 'Solver',
    icon: 'i-mdi-cogs',
    command: () => router.push(`/teacher/${classId.value}/solver`)
},
{
    label: 'Teachers',
    icon: 'i-mdi-account-group',
    command: () => router.push(`/teacher/${classId.value}/teachers`)
},
{
    label: 'Join Code',
    icon: 'i-mdi-link',
    command: () => router.push(`/teacher/${classId.value}/code`)
},
])

</script>
