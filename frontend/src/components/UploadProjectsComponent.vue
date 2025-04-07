<template>
    <div v-if="showModal" class="modal-overlay">
        <div class="modal-content">
            <h2>Upload Project</h2>
            <form @submit.prevent="handleSubmit">
            <div class="form-group">
                <label for="projectDescription">Project Description:</label>
                <text>Please upload a valid csv with the right[insert link to valid format or something idk] with the projects to upload in it.
                    To upload the file, click browse and find the csv, then click the upload button
                </text>
            </div>
    
            <div class="form-group">
                <label for="fileInput">Upload Project File:</label>
                <input
                id="fileInput"
                type="file"
                accept=".csv"
                required
                @change="handleFileChange"
                />
            </div>
    
            <!-- TODO: Align correctly -->
            <div class="form-actions"> 
                <button type="button" @click="closeModal">Cancel</button>
                <button type="submit" :disabled="isSubmitting">Upload</button>
            </div>
            </form>
            <p v-if="errorMessage" class="error-message">{{ errorMessage }}</p>
        </div>
    </div>
</template>
  
<script lang="ts">
    import { defineComponent, ref } from "vue";
    
    interface FormData {
        projectTitle: string;
        projectDescription: string;
        projectFile: File | null;
    }
    
    export default defineComponent({
        name: "ProjectUploadForm",
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
    
        const handleFileChange = (event: Event) => {
            const fileInput = event.target as HTMLInputElement;
            if (fileInput.files && fileInput.files.length > 0) {
            form.value.projectFile = fileInput.files[0];
            }
        };
    
        const handleSubmit = async () => {
            if(!props.showModal){
                closeModal()
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
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
  }
  
  .modal-content {
    background: black;
    padding: 2rem;
    border-radius: 8px;
    width: 400px;
    max-width: 100%;
  }
  
  h2 {
    margin-bottom: 1rem;
    font-size: 1.5rem;
  }
  
  .form-group {
    margin-bottom: 1rem;
  }
  
  label {
    display: block;
    font-weight: bold;
    margin-bottom: 0.5rem;
  }
  
  input,
  
  textarea {
    resize: vertical;
    height: 100px;
  }
  
  button {
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    background-color: #007bff;
    color: white;
    font-size: 1rem;
  }
  
  button:disabled {
    background-color: #cccccc;
  }
  
  button[type="button"] {
    background-color: #f44336;
  }
  
  .error-message {
    color: red;
    font-size: 0.9rem;
    margin-top: 1rem;
  }
  </style>