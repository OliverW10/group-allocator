<script setup lang="ts">
import Navbar from "./components/Navbar.vue"
// Add admin verification here
</script>

<template>
  <!-- Connect navbar to admin verificartion -->
  <Navbar/>
  <router-view></router-view>
</template>
