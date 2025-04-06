<template>
    <div class="project-details-view">
        <ProgressSpinner v-if="loading" />
        <Message severity="error" v-if="!loading && !projectDetails" message="Failed to load project details." />
        <Card v-else>
            <h1>{{ projectDetails?.name }}</h1>
        </Card>
    </div>
</template>
<script setup lang="ts">
import { onMounted, ref } from 'vue';
import type { ProjectDto } from '../dtos/project-dto';
import { Card, Message } from 'primevue';
import ProjectService from '../services/ProjectService';

const props = defineProps({
  projectId: {
    type: String,
    required: true
  }
})

const loading = ref(true)

const projectDetails = ref(null as ProjectDto | null)

const fetchProjectDetails = async () => {
  try {
    projectDetails.value = await ProjectService.getProjectById(props.projectId);
  } catch (error) {
    console.error('Error fetching project details:', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchProjectDetails();
});


</script>