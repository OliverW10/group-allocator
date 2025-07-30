<template>
  <div v-if="show" class="mb-6 p-4 border rounded-xl shadow-sm" :class="warningClasses">
    <div class="flex items-center gap-3">
      <div>
        <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20" :class="iconColorClass">
          <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
        </svg>
      </div>
      <div>
        <h3 class="text-sm font-medium" :class="titleColorClass">{{ title }}</h3>
        <p class="text-sm mt-1" :class="messageColorClass">{{ message }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  show?: boolean;
  severity?: 'warning' | 'error' | 'info';
  title: string;
  message: string;
}

const props = withDefaults(defineProps<Props>(), {
  show: true,
  severity: 'warning'
});

const warningClasses = computed(() => {
  switch (props.severity) {
    case 'error':
      return 'bg-red-50 border-red-200';
    case 'info':
      return 'bg-blue-50 border-blue-200';
    case 'warning':
    default:
      return 'bg-amber-50 border-amber-200';
  }
});

const iconColorClass = computed(() => {
  switch (props.severity) {
    case 'error':
      return 'text-red-600';
    case 'info':
      return 'text-blue-600';
    case 'warning':
    default:
      return 'text-amber-600';
  }
});

const titleColorClass = computed(() => {
  switch (props.severity) {
    case 'error':
      return 'text-red-800';
    case 'info':
      return 'text-blue-800';
    case 'warning':
    default:
      return 'text-amber-800';
  }
});

const messageColorClass = computed(() => {
  switch (props.severity) {
    case 'error':
      return 'text-red-700';
    case 'info':
      return 'text-blue-700';
    case 'warning':
    default:
      return 'text-amber-700';
  }
});
</script> 