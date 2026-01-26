<template>
	<div v-if="histogram.length > 0" class="mt-6">
		<h3 class="text-lg font-semibold mb-3">Preference Distribution</h3>
		<div class="bg-white border border-gray-200 rounded-lg p-4">
			<svg :width="400" :height="200" class="w-full">
				<!-- Chart background -->
				<rect width="400" height="200" fill="var(--p-surface-0, #fff)" />
				
				<!-- Grid lines -->
				<g v-for="i in 5" :key="`grid-${i}`">
					<line 
						:x1="50" 
						:y1="40 + (i-1)*30" 
						:x2="350" 
						:y2="40 + (i-1)*30" 
						stroke="var(--p-surface-200, #e5e7eb)" 
						stroke-width="1" 
					/>
				</g>
				
				<!-- Y-axis labels -->
				<text x="10" y="40" font-size="10" fill="var(--p-text-color-secondary, #888)" text-anchor="end">0</text>
				<text x="10" y="70" font-size="10" fill="var(--p-text-color-secondary, #888)" text-anchor="end">{{ maxValue }}</text>
				
				<!-- X-axis labels -->
				<g v-for="i in histogram.length" :key="`x-label-${i}`">
					<text 
						:x="50 + (i-1)*30" 
						y="190" 
						font-size="10" 
						fill="var(--p-text-color-secondary, #888)" 
						text-anchor="middle"
					>
						{{ i }}
					</text>
				</g>
				
				<!-- Bars -->
				<g v-for="(value, index) in histogram" :key="`bar-${index}`">
					<rect
						:x="50 + index * 30"
						:y="170 - (maxValue > 0 ? (value / maxValue) * 120 : 0)"
						width="20"
						:height="maxValue > 0 ? (value / maxValue) * 120 : 0"
						fill="var(--p-primary-color, #1976d2)"
						opacity="0.8"
					/>
					<text
						:x="60 + index * 30"
						:y="175 - (maxValue > 0 ? (value / maxValue) * 120 : 0)"
						font-size="10"
						fill="var(--p-text-color, #000)"
						text-anchor="middle"
					>
						{{ value }}
					</text>
				</g>
				
				<!-- Axes -->
				<line x1="50" y1="40" x2="50" y2="170" stroke="var(--p-surface-400, #888)" stroke-width="2" />
				<line x1="50" y1="170" x2="350" y2="170" stroke="var(--p-surface-400, #888)" stroke-width="2" />
			</svg>
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
	histogram: number[];
}

const props = defineProps<Props>();

const maxValue = computed(() => {
	return props.histogram.length > 0 ? Math.max(...props.histogram) : 0;
});
</script> 