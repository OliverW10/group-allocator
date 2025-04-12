<template>
    <FileUpload mode="basic" name="file" :auto="false" accept=".csv,.txt" choose-label="Choose File" @select="onSelect" />
    <Button icon="pi pi-question-circle" label="Help" class="p-button-text ml-3" @click="showHelp = true" />

    <Dialog v-model:visible="showHelp" header="File Upload Help" modal>
        <p>
            Please upload a csv file with the following format and no header
            <br />
            <code>project name, client, min_students, max_students, requires_nda</code>
            <br />
            New projects will be added in addition to existing ones.
        </p>
    </Dialog>
</template>

<script setup>
import { ref } from 'vue'
import FileUpload from 'primevue/fileupload'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import ApiService from '../services/ApiService'
import { useToast } from "primevue/usetoast";

const emit = defineEmits(['projectsChanged'])

const showHelp = ref(false)
const toast = useToast();

const onSelect = async (event) => {
    if (!event.files) {
        console.error('selected no file, ignoring')
    }
    const selectedFile = event.files[0]
    const formData = new FormData()
    formData.append('file', selectedFile)
    try{
        const result = await ApiService.postRaw('projects/upload', formData) // , 'multipart/form-data'
        emit('projectsChanged', result)
        if (result != undefined){
            toast.add({ severity: 'success', summary: 'Success', detail: 'Projects uploaded', life: 5000 });
        } else {
            error();
        }
    } catch {
        error();
    }
}

const error = () => {
    toast.add({severity: 'error', summary: 'Error', detail: 'Projects file upload failed', life: 10000})
}

</script>
