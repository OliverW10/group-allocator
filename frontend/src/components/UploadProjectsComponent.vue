<template>
  <Dialog v-model:visible="showModal" header="Upload Project" :modal="true" :closable="false" :style="{ width: '400px' }">
    <form @submit.prevent="handleSubmit">
      <div class="p-field">
        <label for="projectDescription">Project Description:</label>
        <p>
          Please upload a valid CSV with the right 
          <a href="#" target="_blank">format</a> containing the projects to upload.
          To upload the file, click browse and find the CSV, then click the upload button.
        </p>
      </div>

      <div class="p-field">
        <label for="fileInput">Upload Project File:</label>
        <FileUpload
          id="fileInput"
          mode="basic"
          name="file"
          accept=".csv"
          auto="false"
          chooseLabel="Browse"
          @select="handleFileChange"
        />
      </div>

      <div class="p-d-flex p-jc-between p-mt-3">
        <Button label="Cancel" icon="pi pi-times" class="p-button-danger" @click="closeModal" />
        <Button label="Upload" icon="pi pi-upload" :disabled="isSubmitting" type="submit" />
      </div>
    </form>
    <p v-if="errorMessage" class="p-error p-mt-3">{{ errorMessage }}</p>
  </Dialog>
</template>

<script lang="ts">
import { defineComponent, ref } from "vue";
import { Dialog } from "primevue/dialog";
import { Button } from "primevue/button";
import { FileUpload } from "primevue/fileupload";

interface FormData {
  projectTitle: string;
  projectDescription: string;
  projectFile: File | null;
}

export default defineComponent({
  name: "ProjectUploadForm",
  components: {
    Dialog,
    Button,
    FileUpload,
  },
  props: {
    showModal: {
      type: Boolean,
      required: true,
    },
  },
  emits: ["close", "upload"],
  setup(props, { emit }) {
    const form = ref<FormData>({
      projectTitle: "",
      projectDescription: "",
      projectFile: null,
    });
    const isSubmitting = ref(false);
    const errorMessage = ref("");

    const handleFileChange = (event: any) => {
      form.value.projectFile = event.files[0];
    };

    const handleSubmit = async () => {
      if (!props.showModal) {
        closeModal();
      }
      if (!form.value.projectFile) {
        errorMessage.value = "Please select a project file.";
        return;
      }

      isSubmitting.value = true;
      errorMessage.value = "";

      try {
        // Simulate form submission (you can replace this with actual logic like API calls)
        await new Promise((resolve) => setTimeout(resolve, 2000));

        // Emit the form data to the parent component
        emit("upload", form.value);
        closeModal();
      } catch (error) {
        errorMessage.value = "An error occurred while uploading the project.";
        console.error(error);
      } finally {
        isSubmitting.value = false;
      }
    };

    const closeModal = () => {
      emit("close");
    };

    return {
      form,
      isSubmitting,
      errorMessage,
      handleFileChange,
      handleSubmit,
      closeModal,
    };
  },
});
</script>

<style scoped>
.p-error {
  color: red;
  font-size: 0.9rem;
}
</style>
