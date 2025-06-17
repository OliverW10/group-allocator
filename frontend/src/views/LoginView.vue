<template>
	<div class="min-h-screen flex flex-col">
		<!-- Login Buttons Section -->
		<div class="flex justify-end gap-4 p-4">
			<Button class="flex items-center" icon="pi pi-user" label="Sign in as Teacher" @click="loginGoogle('teacher')" />
			<Button class="flex items-center" icon="pi pi-users" label="Sign in as Student" @click="loginGoogle('student')" severity="success" />
		</div>

		<!-- Main Content -->
		<div class="flex-grow flex flex-col items-center justify-center p-8">
			<Message v-if="_window.location.hash" severity="contrast" icon="i-mdi-account-check" class="w-full max-w-md">
				Login Successful, Redirecting...
			</Message>
			<div v-else class="text-center">
				<h1 class="text-4xl font-bold mb-6">Group Allocator</h1>
				<div class="max-w-2xl mx-auto space-y-4">
					<p class="text-xl">Streamline your group project management</p>
					<ul class="text-lg space-y-2">
						<li class="flex items-center justify-center">
							<i class="i-mdi-check-circle text-green-500 mr-2"></i>
							Collect student group preferences
						</li>
						<li class="flex items-center justify-center">
							<i class="i-mdi-check-circle text-green-500 mr-2"></i>
							Automatically group students and assign projects
						</li>
						<li class="flex items-center justify-center">
							<i class="i-mdi-check-circle text-green-500 mr-2"></i>
							Apply manual constraints and customize algorithm
						</li>
					</ul>
				</div>
			</div>
		</div>

		<!-- Dev Login Section -->
		<div v-if="is_dev" class="p-4 border-t border-gray-200">
			<div class="max-w-md mx-auto">
				<h2 class="text-xl font-semibold mb-4">Development Login</h2>
				<div class="space-y-4">
					<div class="flex gap-2">
						<label for="devNameInput">Name:</label>
						<InputText id="devNameInput" v-model="devName" type="text" />
					</div>
					<div class="flex gap-2">
						<label for="devEmailInput">Email:</label>
						<InputText id="devEmailInput" v-model="devEmail" type="email" />
					</div>
					<div class="flex gap-2 items-center">
						<label for="devAdminInput">Role:</label>
						<SelectButton v-model="devLoginType" name="selection" :options="devLoginOptions" />
					</div>
					<Button class="flex items-center" icon="pi pi-cog" label="Development Test Login" @click="loginForDev" severity="secondary" />
				</div>
			</div>
		</div>
	</div>
</template>
<script setup lang="ts">
import router from '../router';
import { useAuthStore } from '../store/auth';
import { getOidcUrl } from "../helpers/oauth";
import { onMounted, ref } from 'vue';
import type { UserInfoDto } from '../dtos/user-info-dto';
import ApiService from '../services/ApiService';
import SelectButton from 'primevue/selectbutton';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Message from 'primevue/message';

// Types
type LoginType = "admin" | "student" | "teacher";

const authStore = useAuthStore();
const devName = ref(`Dummy User ${crypto.randomUUID().slice(0, 2)}`);
const devEmail = ref(`email${crypto.randomUUID().slice(0, 6)}@domain.com`);
const devLoginType = ref('student' as LoginType);
const devLoginOptions = ref(["admin", "student", "teacher"] as LoginType[]);

const is_dev = import.meta.env.DEV;

function getGoogleIdToken() {
	const hash = window.location.hash;
	if (!hash) return null;
	const params = new URLSearchParams(hash.replace('#', '?'));
	return params.get('id_token');
}

const loginGoogle = async (role: 'teacher' | 'student') => {
	location.href = getOidcUrl(role);
};

function getOidcState() {
	const hash = window.location.hash;
	if (!hash) return null;
	const params = new URLSearchParams(hash.replace('#', '?'));
	const state = params.get('state');
	if (!state) return null;
	try {
		return JSON.parse(state);
	} catch {
		return null;
	}
}

onMounted(async () => {
	const id_token = getGoogleIdToken();
	const state = getOidcState();
	if (id_token && state?.role) {
		await loginWithGoogle(id_token, state.role);
	}
});

async function loginWithGoogle(id_token: string, role: 'teacher' | 'student') {
	let endpoint = '';
	let redirect = '';
	if (role === 'teacher') {
		endpoint = `/auth/login-google-teacher?idToken=${id_token}`;
		redirect = '/teacher';
	} else {
		endpoint = `/auth/login-google-student?idToken=${id_token}`;
		redirect = '/class-select';
	}
	authStore.userInfo = await ApiService.get<UserInfoDto>(endpoint);
	router.push(redirect);
}

async function loginForDev() {
	let endpoint = '';
	let redirect = '';
	endpoint = `/auth/login-dev?name=${devName.value}&email=${devEmail.value}&role=${devLoginType.value}`;
	switch (devLoginType.value) {
		case 'teacher':
			redirect = '/teacher';
			break;
		case 'student':
			redirect = '/class-select';
			break;
		case 'admin':
			redirect = '/admin';
			break;
	}
	authStore.userInfo = await ApiService.get<UserInfoDto>(endpoint);
	router.push(redirect);
}

// can't access globals from template
const _window = window;
</script>
