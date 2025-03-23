<script setup lang="ts">
import router from '../router';
import { useAuthStore } from '../store/auth';
import { getOidcUrl } from "../helpers/oauth";

if (window.location.hash) {
  // send it to /auth/login?
  // save response in auth store
  // redirect to clear url
}

const authStore = useAuthStore();
const isLoggedIn = authStore.userInfo;
if (isLoggedIn) {
  const isAdmin = authStore.userInfo?.isAdmin ?? false;
  if (isAdmin) {
    router.push({ name: 'Dashboard' });
  } else {
    router.push({ name: 'Form' });
  }
}
const is_dev = import.meta.env.DEV;

const navigateToOidc = () =>{
  console.log("yep");
  location.href = getOidcUrl();
};

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