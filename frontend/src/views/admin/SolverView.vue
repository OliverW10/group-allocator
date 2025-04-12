<template>
    <AdminNavBar />
    <Divider />
    <Button label="Run Solver"></Button>
    <!-- <Listbox  /> -->
    <!-- <DataTable :value="students" :loading="loading" :paginator="true" :rows="30" :rows-per-page-options="[30, 100]">
        <Column field="email" header="Email"></Column>
        <Column field="willSignContract" header="Contract?"></Column>
        <Column field="orderedPreferences" header="Preferences"></Column>
        <Column field="id" header="Actions">
            <template #body="slotProps">
                <Button label="X" class="p-button-text" @click="remove(slotProps.data.id)" />
            </template> 
        </Column>
    </DataTable> -->
</template>
<script setup lang="ts">
import AdminNavBar from '../../components/AdminNavBar.vue';
import Button from 'primevue/button';
import Divider from 'primevue/divider';

</script>