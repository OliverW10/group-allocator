<template>
    <div>
        <h1 class="heading">Projects</h1>
    </div>
</template>

<script setup lang="ts">
import type ProjectDto from '../models/ProjectDto';
import { ref } from 'vue';


const projectData = ref(
    [
    {
        id: 1,
        name: "Project 1",
        description: "Description 1",
    }] as ProjectDto[]);
</script>