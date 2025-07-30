<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
    <h2 class="text-xl font-semibold text-gray-900 mb-4">Preference Configuration</h2>
    <div class="flex flex-col gap-4">
      <div>
        <label for="preference-exponent" class="block text-sm font-medium text-gray-700 mb-2">
          Preference Exponent: <span class="text-blue-600 font-semibold">{{ preferenceExponent.toFixed(2) }}</span>
        </label>
        <Slider 
          id="preference-exponent"
          v-model="preferenceExponent" 
          :min="0.6" 
          :max="0.99" 
          :step="0.01"
          class="w-full"
          @update:model-value="handleExponentChange"
        />
      </div>
      
      <!-- Preference Curve Visualization -->
      <div class="mt-4">
        <div class="bg-gray-50 rounded-lg p-4 border border-gray-200">
          <svg :width="280" :height="140" class="mx-auto">
            <!-- Background grid -->
            <defs>
              <pattern id="grid" width="17" height="18" patternUnits="userSpaceOnUse">
                <path d="M 17 0 L 0 0 0 18" fill="none" stroke="#e5e7eb" stroke-width="0.5"/>
              </pattern>
            </defs>
            <rect width="100%" height="100%" fill="url(#grid)" />
            
            <!-- Axes -->
            <line x1="40" y1="20" x2="40" y2="120" stroke="#9ca3af" stroke-width="2" />
            <line x1="40" y1="120" x2="260" y2="120" stroke="#9ca3af" stroke-width="2" />
            
            <!-- X axis labels -->
            <g v-for="x in 10" :key="x">
              <text :x="40 + (x-1)*22" y="135" font-size="11" text-anchor="middle" fill="#6b7280" font-weight="500">{{ x }}</text>
            </g>
            
            <!-- Y axis labels -->
            <text x="15" y="120" font-size="11" fill="#6b7280" font-weight="500">0</text>
            <text x="15" y="25" font-size="11" fill="#6b7280" font-weight="500">1</text>
            
            <!-- Curve -->
            <polyline
              :points="svgPoints"
              fill="none"
              stroke="#3b82f6"
              stroke-width="3"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
        </div>
        <p class="text-xs text-gray-500 mt-2 text-center">
          Higher values give more weight to student preferences
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import Slider from 'primevue/slider';

interface Props {
  modelValue?: number;
}

interface Emits {
  (e: 'update:modelValue', value: number): void;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: 0.85
});

const emit = defineEmits<Emits>();

const preferenceExponent = ref(props.modelValue);

// Watch for external changes to the prop
watch(() => props.modelValue, (newValue) => {
  preferenceExponent.value = newValue;
});

const handleExponentChange = (value: number | number[]) => {
  const numValue = Array.isArray(value) ? value[0] : value;
  preferenceExponent.value = numValue;
  emit('update:modelValue', numValue);
};

const svgPoints = computed(() => {
  const m = preferenceExponent.value;
  const points: string[] = [];
  const substeps = 5;
  for (let x = 0; x <= 10 * substeps; x++) {
    // Calculate y = m^x, clamp to [0,1] for display
    const y = Math.pow(m, x);
    // Map x to [40, 260], y to [120, 20] (invert y for SVG)
    const svgX = 40 + x * 22 / substeps;
    const svgY = 120 - y * 100;
    points.push(`${svgX},${svgY}`);
  }
  return points.join(' ');
});
</script> 