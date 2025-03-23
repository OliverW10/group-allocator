<script setup lang="ts">
import router from '../router';
import { useAuthStore } from '../store/auth';
import { getOidcUrl } from "../helpers/oauth";
import { onMounted } from 'vue';
import type { UserInfoDto } from '../dtos/user-info-dto';

const authStore = useAuthStore();

const is_dev = import.meta.env.DEV;

const navigateToOidc = () =>{
  location.href = getOidcUrl();
};

onMounted(async ()=>{
  if (window.location.hash) {
    const id_token = new URLSearchParams(window.location.hash).get("id_token")
    // TODO: replace with ApiService
    const backendUrl = "https://localhost:7000"
    const result = await fetch(`${backendUrl}/auth/login-google?idToken=${id_token}`)
    authStore.userInfo = (await result.json()) as UserInfoDto;
  }
  redirectIfLoggedIn();
});

const redirectIfLoggedIn = () => {
  const isLoggedIn = authStore.userInfo;
  if (isLoggedIn) {
    const isAdmin = authStore.userInfo?.isAdmin ?? false;
    if (isAdmin) {
      router.push('/form');
    } else {
      router.push('/dashboard');
    }
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
    <div v-if="is_dev">
      <button class="flex items-center w-max p-3 rounded-md">
        Development Test Login
        <i class="i-mdi-robot ml-2"></i>
      </button>
    </div>
  </div>
</template>