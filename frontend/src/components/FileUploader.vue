<template>
    <span class="p-4">
        <FileUpload mode="basic" name="demo[]" url="/api/upload" accept=".csv" :maxFileSize="1000000" @upload="onUpload" :auto="true" chooseLabel="Browse" />

        <Button icon="pi pi-question-circle" label="Help" class="p-button-text ml-3" @click="showHelp = true" />

        <Dialog header="File Upload Help" v-model:visible="showHelp" modal>
            <p>
                Please upload a csv file with the following format and no header
                <br />
                <code>project name, client, min_students, max_students, requires_nda</code>
                <br />
                New projects will be added in addition to existing ones.
            </p>
        </Dialog>
    </span>
</template>

<script setup>
import { ref } from 'vue'
import FileUpload from 'primevue/fileupload'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'

const showHelp = ref(false)

const onUpload = () => {

}
</script>
