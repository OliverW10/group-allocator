<template>
	<div class="flex flex-col gap-4 p-4">
		<header class="flex justify-between items-center">
			<h1 class="heading">Select a Class</h1>
			<p>Logged in as {{ userInfo?.name }}</p>
		</header>

		<div class="flex gap-8">
			<!-- Left Panel: List of Classes -->
			<div class="flex-1">
				<Card>
					<template #title>Your Classes</template>
					<template #content>
						<ProgressBar v-if="loading" mode="indeterminate" style="height: 6px" />
						<div v-else-if="classes.length === 0" class="text-center p-4">
							<p>You are not enrolled in any classes yet.</p>
						</div>
						<div v-else class="flex flex-col gap-2">
							<Button v-for="classItem in classes" 
								:key="classItem.id" 
								class="w-full text-left p-3"
								@click="joinClass(classItem.id)">
								<div class="flex flex-col">
									<span class="font-bold">{{ classItem.name }}</span>
									<span class="text-sm text-gray-600">Code: {{ classItem.code }}</span>
								</div>
							</Button>
						</div>
					</template>
				</Card>
			</div>

			<!-- Right Panel: Join with Code -->
			<div class="flex-1">
				<Card>
					<template #title>Join with Class Code</template>
					<template #content>
						<div class="flex flex-col gap-4">
							<InputText v-model="classCode" 
								placeholder="Enter 5-letter class code" 
								:class="{ 'p-invalid': classCodeError }"
								@keyup.enter="joinWithCode" />
							<small v-if="classCodeError" class="p-error">{{ classCodeError }}</small>
							<Button label="Join Class" 
								:loading="joining" 
								@click="joinWithCode" />
						</div>
					</template>
				</Card>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import Card from 'primevue/card';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import ProgressBar from 'primevue/progressbar';
import ApiService from '../../services/ApiService';
import type { ClassResponseDto } from '../../dtos/class-response-dto';
import type { UserInfoDto } from '../../dtos/user-info-dto';

const router = useRouter();
const toast = useToast();

const loading = ref(false);
const joining = ref(false);
const classes = ref<ClassResponseDto[]>([]);
const userInfo = ref<UserInfoDto | null>(null);
const classCode = ref('');
const classCodeError = ref('');

onMounted(async () => {
	loading.value = true;
	try {
		// Get user info
		userInfo.value = await ApiService.get<UserInfoDto>('/auth/me');
		
		// Get list of classes
		classes.value = await ApiService.get<ClassResponseDto[]>('/class');
	} catch (error) {
		console.error('Failed to load data:', error);
		toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load data', life: 3000 });
	} finally {
		loading.value = false;
	}
});

const joinClass = async (classId: number) => {
	joining.value = true;
	try {
		await ApiService.get(`/class/join/${classId}`);
		toast.add({ severity: 'success', summary: 'Success', detail: 'Successfully joined class', life: 3000 });
		router.push('/form');
	} catch (error) {
		console.error('Failed to join class:', error);
		toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to join class', life: 3000 });
	} finally {
		joining.value = false;
	}
};

const joinWithCode = async () => {
	if (!classCode.value) {
		classCodeError.value = 'Please enter a class code';
		return;
	}

	if (classCode.value.length !== 5) {
		classCodeError.value = 'Class code must be 5 letters';
		return;
	}

	joining.value = true;
	classCodeError.value = '';
	
	try {
		await ApiService.get(`/class/join-code/${classCode.value}`);
		toast.add({ severity: 'success', summary: 'Success', detail: 'Successfully joined class', life: 3000 });
		router.push('/form');
	} catch (error) {
		console.error('Failed to join class:', error);
		toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to join class', life: 3000 });
	} finally {
		joining.value = false;
	}
};
</script>

<style scoped>
.p-card {
	height: 100%;
}
</style>

