<script setup lang="ts">
import router from '../router';
import { useAuthStore } from '../store/auth';
import { getOidcUrl } from "../helpers/oauth";
import { onMounted } from 'vue';
import type { UserInfoDto } from '../dtos/user-info-dto';
import ApiService from '../services/ApiService';

const authStore = useAuthStore();
const devName = defineModel<string>("name");
const devEmail = defineModel<string>("email");
const devIsAdmin = defineModel<boolean>("isAdmin")

devName.value = "Dummy User"
devEmail.value = `email${crypto.randomUUID().slice(0, 6)}@domain.com`

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
  login(`/auth/login-google?idToken=${id_token}`);
}

async function loginForDev() {
  login(`/auth/login-dev?name=${devName.value}&email=${devEmail.value}&isAdmin=${devIsAdmin.value}`);
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

</script>

<template>
  <div class="flex flex-col justify-center h-screen">
    <div class="flex flex-col items-center border-neutral-600 border-2 mx-auto p-8 rounded-lg shadow-lg min-w-lg">
      <h1 class="heading py-4">Group Allocator</h1>
      <div>
        <button class="flex items-center w-max p-3 rounded-md m-3" @click="navigateToOidc">
          Sign in with Google
          <i class="i-logos-google-icon ml-2"></i>
        </button>
      </div>
      <div v-if="is_dev" class="flex flex-col gap-4">
        <hr class="my-4" />
        <div class="flex gap-2">
          <label for="devNameInput">Name:</label>
          <input id="devNameInput" v-model="devName" type="text" class="border">
        </div>

        <div class="flex gap-2">
          <label for="devEmailInput">Email:</label>
          <input id="devEmailInput" v-model="devEmail" type="email" class="border">
        </div>

        <div class="flex gap-2">
          <label for="devAdminInput">Admin:</label>
          <input id="devAdminInput" v-model="devIsAdmin" type="checkbox">
        </div>

        <button class="flex items-center w-max p-3 rounded-md" @click="loginForDev">
          Development Test Login
          <i class="i-mdi-robot ml-2"></i>
        </button>
      </div>
    </div>
  </div>
</template>