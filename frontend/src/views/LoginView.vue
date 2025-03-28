<script setup lang="ts">
import router from '../router';
import { useAuthStore } from '../store/auth';
import { getOidcUrl } from "../helpers/oauth";
import { onMounted } from 'vue';
import type { UserInfoDto } from '../dtos/user-info-dto';

const authStore = useAuthStore();
const devName = defineModel<string>("name");
const devEmail = defineModel<string>("email");
const devIsAdmin = defineModel<boolean>("isAdmin")

devName.value = "Dummy User"
devEmail.value = "email@domain.com"

const is_dev = import.meta.env.DEV;

const navigateToOidc = () => {
  location.href = getOidcUrl();
};

onMounted(async () => {
  if (window.location.hash) {
    await loginWithGoogle();
  }
});

const backendUrl = import.meta.env.VITE_BACKEND_URL;

async function loginWithGoogle() {
  const id_token = new URLSearchParams(window.location.hash).get("id_token");
  login(`${backendUrl}/auth/login-google?idToken=${id_token}`);
}

async function loginForDev() {
  login(`${backendUrl}/auth/login-dev?name=${devName.value}&email=${devEmail.value}&isAdmin=${devIsAdmin.value}`);
}

async function login(url: string) {
  const result = await fetch(url, {
    credentials: "include"
  });
  authStore.userInfo = (await result.json()) as UserInfoDto;
  const isAdmin = authStore.userInfo?.isAdmin ?? false;
  if (isAdmin) {
    router.push('/dashboard');
  } else {
    router.push('/form');
  }
}

</script>

<template>
  <div class="flex flex-col justify-center items-center">
    <h1 class="heading py-4">My Account</h1>
    <div>
      <button class="flex items-center w-max p-3 rounded-md m-3" @click="navigateToOidc">
        Sign in with Google
        <i class="i-logos-google-icon ml-2"></i>
      </button>
    </div>
    <hr class="border">
    <div v-if="is_dev">
      <label for="devNameInput">Name:</label>
      <input id="devNameInput" v-model="devName" type="text" class="border">

      <label for="devEmailInput">Email:</label>
      <input id="devEmailInput" v-model="devEmail" type="email" class="border">

      <label for="devAdminInput">Admin:</label>
      <input id="devAdminInput" v-model="devIsAdmin" type="checkbox">

      <button class="flex items-center w-max p-3 rounded-md" @click="loginForDev">
        Development Test Login
        <i class="i-mdi-robot ml-2"></i>
      </button>
    </div>
  </div>
</template>