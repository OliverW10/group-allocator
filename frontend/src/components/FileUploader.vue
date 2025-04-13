<template>
    <div class="flex justify-center">
        <FileUpload mode="basic" name="file" :auto="false" accept=".csv,.txt" choose-label="Choose File"
            @select="onSelect" />
        <Button icon="i-mdi-help-circle" label="Help" class="p-button-text ml-3" severity="info"
            @click="showHelp = true" />
    </div>

    <Dialog v-model:visible="showHelp" header="File Upload Help" modal :draggable="false">
        <slot></slot>
    </Dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import FileUpload, { type FileUploadSelectEvent } from 'primevue/fileupload'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'

const emit = defineEmits(['projectsChanged'])

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