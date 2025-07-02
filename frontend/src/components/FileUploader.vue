<template>
    <div class="flex justify-center">
        <div class="flex">
            <FileUpload mode="basic" name="file" :auto="false" accept=".csv,.txt" :choose-label=props.label
                @select="onSelect" />
            <Button icon="i-mdi-help-circle" label="Help" class="p-button-text" severity="info"
                @click="showHelp = true" />
        </div>
    </div>

    <Dialog v-model:visible="showHelp" header="File Upload Help" modal :draggable="false" :style="{ width: '50vw' }">
        <slot></slot>
    </Dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import FileUpload, { type FileUploadSelectEvent } from 'primevue/fileupload'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'

const emit = defineEmits(['projectsChanged'])
const props = defineProps<{
  label: string
}>()

const showHelp = ref(false)

const onSelect = async (event: FileUploadSelectEvent) => {
    emit('projectsChanged', event)
}

</script>

<style>
.p-fileupload-basic>span {
    display: none;
}
</style>
