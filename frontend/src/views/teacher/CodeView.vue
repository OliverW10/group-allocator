<template>
    <div class="min-h-screen bg-gray-50">
        <!-- Back Button -->
        <div class="p-4">
            <button 
                class="flex items-center gap-2 text-gray-600 hover:text-gray-800 transition-colors"
                @click="goBack" 
            >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"></path>
                </svg>
                Back
            </button>
        </div>

        <!-- Main Content -->
        <div class="flex items-center justify-center min-h-[calc(100vh-80px)]">
            <div class="text-center">
                <!-- Loading State -->
                <div v-if="loading" class="text-center">
                    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                    <p class="mt-4 text-gray-600">Loading class information...</p>
                </div>

                <!-- Error State -->
                <div v-else-if="error" class="text-center">
                    <div class="text-red-600 text-6xl mb-4">⚠️</div>
                    <h2 class="text-2xl font-semibold text-gray-800 mb-2">Error Loading Class</h2>
                    <p class="text-gray-600 mb-4">{{ error }}</p>
                    <button 
                        @click="fetchClassInfo" 
                        class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
                    >
                        Try Again
                    </button>
                </div>

                <!-- Success State -->
                <div v-else-if="classInfo" class="text-center">
                    <div class="bg-white rounded-lg shadow-lg p-8 max-w-md mx-auto">
                        <h1 class="text-4xl font-bold text-gray-800 mb-2">Class Code</h1>
                        <div 
                            @click="copyCode"
                            class="bg-blue-50 border-2 border-blue-200 rounded-lg p-6 mb-6 cursor-pointer hover:bg-blue-100 hover:border-blue-300 transition-colors relative group"
                        >
                            <div class="text-6xl font-mono font-bold text-blue-600 tracking-wider">
                                {{ classInfo.code }}
                            </div>
                            <!-- Copy Icon -->
                            <div class="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"></path>
                                </svg>
                            </div>
                        </div>
                        <p class="text-gray-600 text-lg">
                            <span class="font-semibold">{{ classInfo.studentsWithPreferencesCount }} / {{ classInfo.studentCount }}</span> 
                            {{ classInfo.studentCount === 1 ? 'student' : 'students' }} submitted preferences
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import type { ClassInfoDto } from '../../dtos/class-info-dto';
import ApiService from '../../services/ApiService';

const route = useRoute();
const router = useRouter();
const toast = useToast();
const classId = route.params.classId as string;

const classInfo = ref<ClassInfoDto | null>(null);
const loading = ref(true);
const error = ref<string | null>(null);
const intervalId = ref<number | null>(null);

const fetchClassInfo = async () => {
    try {
        error.value = null;
        classInfo.value = await ApiService.get<ClassInfoDto>(`/class/code/${classId}`);
    } catch (err) {
        console.error('Error fetching class info:', err);
        error.value = 'Failed to load class information. Please try again.';
    } finally {
        loading.value = false;
    }
};

const copyCode = async () => {
    if (!classInfo.value?.code) return;
    
    try {
        await navigator.clipboard.writeText(classInfo.value.code);
        toast.add({ 
            severity: 'success', 
            summary: 'Copied!', 
            detail: 'Class code copied to clipboard', 
            life: 3000 
        });
    } catch (err) {
        console.error('Failed to copy code:', err);
        toast.add({ 
            severity: 'error', 
            summary: 'Error', 
            detail: 'Failed to copy code to clipboard', 
            life: 3000 
        });
    }
};

const goBack = () => {
    router.push(`/teacher`);
};

onMounted(() => {
    fetchClassInfo();
    // Set up interval to fetch class info every 5 seconds
    intervalId.value = setInterval(fetchClassInfo, 10000);
});

onUnmounted(() => {
    // Clean up interval when component is unmounted
    if (intervalId.value) {
        clearInterval(intervalId.value);
    }
});
</script>

<style scoped>
/* Additional styles can be added here if needed */
</style>
