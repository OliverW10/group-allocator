<template>
	<div class="min-h-screen flex flex-col">
		<!-- Login Buttons Section -->
		<div class="flex justify-end gap-4 p-4">
			<button class="flex items-center px-4 py-2 rounded-md bg-blue-500 text-white hover:bg-blue-600" @click="navigateToOidc">
				Sign in as Teacher
				<i class="i-mdi-account-tie ml-2"></i>
			</button>
			<button class="flex items-center px-4 py-2 rounded-md bg-green-500 text-white hover:bg-green-600" @click="navigateToOidc">
				Sign in as Student
				<i class="i-mdi-account ml-2"></i>
			</button>
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
						<input id="devNameInput" v-model="devName" type="text" class="border rounded px-2 py-1">
					</div>
					<div class="flex gap-2">
						<label for="devEmailInput">Email:</label>
						<input id="devEmailInput" v-model="devEmail" type="email" class="border rounded px-2 py-1">
					</div>
					<div class="flex gap-2 items-center">
						<label for="devAdminInput">Role:</label>
						<SelectButton v-model="devLoginType" name="selection" :options="devLoginOptions" />
					</div>
					<button class="flex items-center px-4 py-2 rounded-md bg-gray-500 text-white hover:bg-gray-600" @click="loginForDev">
						Development Test Login
						<i class="i-mdi-robot ml-2"></i>
					</button>
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

type LoginType = "admin" | "student" | "teacher";

const authStore = useAuthStore();
const devName = ref(`Dummy User ${crypto.randomUUID().slice(0, 2)}`);
const devEmail = ref(`email${crypto.randomUUID().slice(0, 6)}@domain.com`);
const devLoginType = ref('student' as LoginType);
const devLoginOptions = ref(["admin", "student", "teacher"] as LoginType[]);

const is_dev = import.meta.env.DEV;

const navigateToOidc = () => {
	location.href = getOidcUrl();
};

onMounted(async () => {
	if (window.location.hash) {
		await loginWithGoogle();
	}
});


async function loginWithGoogle() {
	const id_token = new URLSearchParams(window.location.hash).get("id_token");
	await login(`/auth/login-google?idToken=${id_token}`);
}

async function loginForDev() {
	await login(`/auth/login-dev?name=${devName.value}&email=${devEmail.value}&isAdmin=${devLoginType.value == "teacher"}`); // TODO: support admin login
}

async function login(url: string) {
	authStore.userInfo = await ApiService.get<UserInfoDto>(url);
	const isAdmin = authStore.userInfo?.isAdmin ?? false;
	if (isAdmin) {
		router.push('/admin/projects');
	} else {
		router.push('/form');
	}
}

// can't access globals from template
const _window = window;

</script>
