<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mt-6">
    <h2 class="text-xl font-semibold text-gray-900 mb-4">Client Limits</h2>
    <DataTable :value="clientLimits" class="w-full" :rows="10" edit-mode="cell" :row-class="rowClassForClientLimit">
      <Column field="clientId" header="Client">
        <template #body="slotProps">
          <Dropdown 
            v-model="slotProps.data.clientId" 
            :options="allClients" 
            option-label="name" 
            option-value="id" 
            placeholder="Select Client" 
            class="w-full" 
          />
        </template>
      </Column>
      <Column field="minProjects" header="Min Projects">
        <template #body="slotProps">
          <InputNumber v-model="slotProps.data.minProjects" :min="0" class="w-full" />
        </template>
      </Column>
      <Column field="maxProjects" header="Max Projects">
        <template #body="slotProps">
          <InputNumber v-model="slotProps.data.maxProjects" :min="0" class="w-full" />
        </template>
      </Column>
      <Column header="Actions">
        <template #body="slotProps">
          <Button icon="i-mdi-delete" severity="danger" text @click="removeClientLimit(slotProps.index)" />
        </template>
      </Column>
      <template #footer>
        <Button label="Add Client Limit" icon="i-mdi-plus" class="mt-2" @click="addClientLimit" />
      </template>
    </DataTable>
    <div v-if="invalidClientLimits.length > 0" class="mt-2 text-red-600 text-sm">
      <ul>
        <li v-for="(lim, idx) in invalidClientLimits" :key="idx">
          Client {{ allClients.find(c => c.id === lim.clientId)?.name || lim.clientId }}: Max Projects must be greater than or equal to Min Projects.
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Dropdown from 'primevue/dropdown';
import InputNumber from 'primevue/inputnumber';
import type { ClientLimitsDto } from '../../dtos/client-limits-dto';
import type { ClientDto } from '../../dtos/client-dto';

interface Props {
  modelValue: ClientLimitsDto[];
  allClients: ClientDto[];
}

interface Emits {
  (e: 'update:modelValue', value: ClientLimitsDto[]): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const clientLimits = ref<ClientLimitsDto[]>(props.modelValue);

// Watch for external changes to the prop
watch(() => props.modelValue, (newValue) => {
  clientLimits.value = newValue;
});

// Watch for internal changes and emit updates
watch(clientLimits, (newValue) => {
  emit('update:modelValue', newValue);
}, { deep: true });

const addClientLimit = () => {
  clientLimits.value.push({ 
    clientId: props.allClients[0]?.id ?? 0, 
    minProjects: 0, 
    maxProjects: 1 
  });
};

const removeClientLimit = (idx: number) => {
  clientLimits.value.splice(idx, 1);
};

const invalidClientLimits = computed(() =>
  clientLimits.value.filter(lim => lim.maxProjects < lim.minProjects)
);

const rowClassForClientLimit = (data: ClientLimitsDto) => {
  return { 'bg-red-100': data.maxProjects < data.minProjects };
};
</script> 