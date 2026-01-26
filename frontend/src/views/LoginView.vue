<template>
	<div class="min-h-screen">
		<!-- Hero Section -->
		<div class="flex h-screen relative">
			<!-- Left Content Section (2/3 width) -->
			<div class="flex-1 flex flex-col">
				<!-- Login Buttons Section - Mobile only -->
				<div class="md:hidden flex justify-end gap-2 p-4">
					<Button class="flex items-center text-sm" icon="pi pi-user" label="Teacher" @click="loginGoogle('teacher')" />
					<Button class="flex items-center text-sm" icon="pi pi-users" label="Student" severity="success" @click="loginGoogle('student')" />
				</div>

				<!-- Main Content -->
				<div class="flex-grow flex flex-col items-center justify-center p-8">
					<Message v-if="_window.location.hash" severity="contrast" icon="i-mdi-account-check" class="w-full max-w-md">
						Login Successful, Redirecting...
					</Message>
					<div v-else class="text-center">
						<h1 class="text-2xl md:text-5xl font-bold mb-6">Group Allocator</h1>
						
						<!-- Mobile Image (shown only on mobile) -->
						<div class="md:hidden mb-6">
							<div class="bg-white rounded-lg shadow-lg p-4 mx-auto w-75 sm:w-120">
								<img 
									src="/src/assets/screenshot.png" 
									alt="Group Allocator Screenshot" 
									class="w-full h-auto object-contain"
									@error="handleImageError"
								/>
							</div>
						</div>
						
						<div class="max-w-2xl mx-auto space-y-4">
							<!-- <p class="text-lg md:text-xl">Streamline your group project management</p> -->
							<ul class="text-base md:text-2xl space-y-2">
								<li class="flex items-center justify-center">
									<i class="i-mdi-check-circle text-green-500 mr-2"></i>
									Collect student project preferences
								</li>
								<li class="flex items-center justify-center">
									<i class="i-mdi-check-circle text-green-500 mr-2"></i>
									Automatically group students and assign projects
								</li>
								<li class="flex items-center justify-center">
									<i class="i-mdi-check-circle text-green-500 mr-2"></i>
									Customize automatic solver constraints
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

			<!-- Right Background Section (1/3 width) - Hidden on mobile -->
			<div class="hidden md:block w-1/3 bg-blue-600 relative dark:bg-blue-800">
				<!-- Login Buttons Section - Positioned on top of blue section -->
				<div class="absolute top-4 right-4 z-10 flex gap-2">
					<Button class="flex items-center text-sm" label="Sign in Teacher" @click="loginGoogle('teacher')" />
					<Button class="flex items-center text-sm" label="Sign in Student" severity="success" @click="loginGoogle('student')" />
				</div>
				
				<!-- Floating Image -->
				<div class="absolute inset-0 flex items-center justify-center">
					<div class="bg-white rounded-lg shadow-2xl p-8 transform -translate-x-1/3">
						<img 
							src="/src/assets/screenshot.png" 
							alt="Group Allocator Logo" 
							class="object-contain"
							@error="handleImageError"
						/>
					</div>
				</div>
			</div>
		</div>

		<!-- Pricing Section -->
		<div class="bg-gray-50 py-16">
			<div class="max-w-6xl mx-auto px-4">
				<div class="text-center mb-12">
					<h2 class="text-3xl font-bold text-gray-900 mb-4">Pricing</h2>
				</div>
				
				<div class="grid md:grid-cols-2 gap-8 max-w-4xl mx-auto">
					<!-- Free Tier Card -->
					<div class="bg-white rounded-lg shadow-lg p-8 border border-gray-200">
						<div class="text-center">
							<h3 class="text-2xl font-bold text-gray-800 mb-2">Free</h3>
							<div class="text-4xl font-bold 600 mb-6">$0</div>
							<ul class="text-gray-600 space-y-3 mb-8">
								<li class="flex items-center">
									<i class="i-mdi-check text-green-500 mr-3"></i>
									Up to 20 students
								</li>
								<li class="flex items-center">
									<i class="i-mdi-check text-green-500 mr-3"></i>
									Access to all features
								</li>
								<li class="flex items-center">
									<i class="i-mdi-check text-green-500 mr-3"></i>
									Collect unlimited student preferences
								</li>
								<li class="flex items-center">
									<i class="i-mdi-check text-green-500 mr-3"></i>
									Manage unlimited projects
								</li>
							</ul>
						</div>
					</div>
					
					<!-- Pro Tier Card -->
					<div class="bg-white rounded-lg shadow-lg p-8 border border-gray-200">
						<div class="text-center">
							<h3 class="text-2xl font-bold mb-2">Paid</h3>
							<div class="text-4xl font-bold mb-6">$5<span class="text-xl">/class</span></div>
							<ul class="space-y-3 mb-8 opacity-90">
								<li class="flex items-center">
									<i class="i-mdi-check mr-3"></i>
									Unlimited students
								</li>
								<li class="flex items-center">
									<i class="i-mdi-check mr-3"></i>
									Help support hosting costs
								</li>
							</ul>
						</div>
					</div>
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

const is_dev = false; //import.meta.env.DEV;

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

// Handle image loading errors
const handleImageError = (event: Event) => {
	const target = event.target as HTMLImageElement;
	target.style.display = 'none';
};
</script>
