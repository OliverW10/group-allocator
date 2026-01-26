<template>
	<div class="flex flex-col gap-4 p-2 sm:p-4 justify-center items-center min-h-screen">
		<h1 class="heading text-center mb-4 text-xl sm:text-2xl px-2">Submit Preferences</h1>
		<Card class="w-full max-w-5xl mx-2 sm:mx-4">
			<template #content>
				<div class="p-2 sm:p-4">
					<h2 class="text-lg sm:text-xl mb-3">Select Your Project Preferences</h2>

					<ProgressBar v-if="loading" mode="indeterminate" style="height: 6px" />

					<PickList 
						v-if="!loading" 
						v-model:model-value="projects" 
						:list-style="mobilePickListStyle" 
						data-key="id" 
						:dragdrop="!isMobile" 
						:meta-key-selection="true" 
						:show-source-controls="false" 
						v-on:update:model-value="warnIfExceededPreferenceLimit"
						class="mobile-picklist"

					>
						<template #sourceheader><b class="text-base sm:text-lg">Available Projects</b></template>
						<template #targetheader><b class="text-base sm:text-lg">Selected Projects (Ordered)</b></template>

						<template #option="{ option, index }">
							<div class="flex flex-row items-center gap-1 sm:gap-2 w-full">
								<span class="text-sm sm:text-base flex-1">{{ projects[1].includes(option) ? `${index + 1}. ` : '' }}{{ option.name }}</span>
								<div class="flex items-center gap-1">
									<Badge v-if="option?.requiresNda" severity="info" class="text-xs" icon="i-mdi-shield-account">
										NDA
									</Badge>
									<Badge 
										v-tooltip="'Client: ' + option.client + '\nMin Students: ' + option.minStudents + '\nMax Students: ' + option.maxStudents" 
										class="text-xs" 
										size="small" 
										value="?" 
									/>
								</div>
							</div>
						</template>
					</PickList>
					
					<div class="flex flex-col sm:flex-row sm:items-center gap-2 my-4">
						<label for="switch1" class="text-sm sm:text-base flex-1">I am willing to sign an NDA to work on a project</label>
						<ToggleSwitch v-model="willSignContract" input-id="switch1" @update:model-value="updateFilteredProjects" />
					</div>
					<div v-if="!willSignContract && someProjectsRequireAnNda" class="flex gap-2 my-4">
						<Message severity="info" icon="i-mdi-shield-account" class="w-full text-sm">
							Some projects were filtered out because they require an NDA and you are have not selected to sign one.
						</Message>
					</div>

					<FileUpload 
						name="demo[]" 
						url="/api/upload" 
						:multiple="true" 
						:max-file-size="10000000"
						custom-upload 
						@uploader="onUpload($event)"
						class="mobile-fileupload"
					>
						<template #empty>
							<span class="text-sm">Drag and drop files to here to upload.</span>
						</template>
					</FileUpload>

					<DataTable 
						:value="files" 
						:paginator="true" 
						:rows="isMobile ? 5 : 10" 
						class="my-4 mobile-datatable"
						:scrollable="true"
						scroll-height="200px"
					>
						<Column field="name" header="Name" class="text-sm">
							<template #body="slotProps">
								<span class="text-xs sm:text-sm truncate block max-w-32 sm:max-w-none">{{ slotProps.data.name }}</span>
							</template>
						</Column>
						<Column field="actions" header="Actions" class="w-16">
							<template #body="slotProps">
								<Button 
									severity="danger" 
									class="i-mdi-delete p-2" 
									@click="deleteFile(slotProps.data.id)"
									size="small"
								/>
							</template>
						</Column>
					</DataTable>

					<div class="my-4">
						<label for="personalStatement" class="block text-sm font-medium mb-2">Personal Statement (Optional)</label>
						<Textarea 
							id="personalStatement"
							v-model="personalStatement" 
							placeholder="Tell us about yourself, your interests, and why you're interested in these projects (Optional)..."
							:auto-resize="true"
							:rows="isMobile ? 3 : 4"
							class="w-full text-sm"
						/>
					</div>

					<div>
						Friend selector
					</div>

					<Button 
						label="Save Preferences" 
						class="mt-4 w-full sm:w-auto" 
						icon="i-mdi-upload" 
						@click="submitForm"
						size="large"
					/>
				</div>
			</template>
		</Card>
		<LogoutButton />
		
		<!-- Success Dialog -->
		<Dialog 
			v-model:visible="showSuccessDialog" 
			modal 
			header="Success"
			:closable="true"
			:close-on-escape="true"
			:style="{ width: '90vw', maxWidth: '500px' }"
		>
			<p class="text-base mb-2">Your project preferences have been successfully submitted.</p>
			<p class="text-base mb-4">You can return to this page anytime to update your preferences.</p>
			
			<div class="flex flex-col sm:flex-row gap-2 justify-center">
				<Button 
					label="Close" 
					@click="showSuccessDialog = false"
					class="flex-1 sm:flex-none"
					severity="secondary"
				/>
			</div>
		</Dialog>
	</div>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import Badge from "primevue/badge";
import Card from "primevue/card";
import Message from "primevue/message"
import ToggleSwitch from "primevue/toggleswitch";
import FileUpload, { type FileUploadUploaderEvent } from "primevue/fileupload";
import Textarea from "primevue/textarea";
import { ProjectDto } from "../../dtos/project-dto";
import LogoutButton from "../../components/LogoutButton.vue";
import { computed, onMounted, ref, onUnmounted } from 'vue'
import PickList from 'primevue/picklist'
import ProgressBar from 'primevue/progressbar'
import ApiService from "../../services/ApiService";
import { useToast } from "primevue/usetoast";
import type { FileDetailsDto } from "../../dtos/file-details-dto";
import type { StudentSubmissionDto } from "../../dtos/student-submission-dto";
import { Column, DataTable } from "primevue";
import { useRoute } from 'vue-router';
import Dialog from "primevue/dialog";

const toast = useToast();
const route = useRoute();
const classId = parseInt(route.params.classId as string);

const files = ref([] as FileDetailsDto[])
const willSignContract = ref(true)
const personalStatement = ref("")
const maxNumberOfPreferences = 10;
const warningMessage = "A maximum of " + maxNumberOfPreferences + " preferences has been selected. Anything more than the top " + maxNumberOfPreferences + " preferences will not be saved";
let exceededPreferenceLimit = false
const loading = ref(false)
const showSuccessDialog = ref(false)

const projectsRaw = ref([] as ProjectDto[]);
const projects = ref([[], []] as ProjectDto[][]);
const someProjectsRequireAnNda = computed(() => projectsRaw.value.some(x => x.requiresNda))

// Mobile detection
const isMobile = ref(false)
const checkMobile = () => {
	isMobile.value = window.innerWidth < 768
}
const mobilePickListStyle = computed(() => isMobile.value ? "min-height: 300px" : "min-height: 500px")

onMounted(async () => {
	checkMobile()
	window.addEventListener('resize', checkMobile)
	
	loading.value = true
	try{
		await loadProjects()
		const maybeStudent = await ApiService.get<StudentSubmissionDto | undefined>(`/students/me?classId=${classId}`)
		if (maybeStudent) {
			console.log(maybeStudent)
			files.value = maybeStudent.files ?? []
			willSignContract.value = true
			personalStatement.value = maybeStudent.notes ?? ""
			const isSelected = (x: ProjectDto) => maybeStudent.orderedPreferences.some(id => x.id == id)
			projects.value[0] = projectsRaw.value.filter(not(isSelected))
			projects.value[1] = projectsRaw.value.filter(isSelected)
			toast.add({ severity: 'success', summary: 'Success', detail: 'Loaded previous submission', life: 3000 });
		}
		loading.value = false
	} catch {
		toast.add({ severity: 'error', summary: 'Error', detail: 'Something went wrong fetching previous submission', life: 3000 });
	}
})

const loadProjects = async () => {
	try {
		loading.value = true

		const allProjects = await ApiService.get<ProjectDto[]>(`/projects?classId=${classId}`)
		if (allProjects.length == 0) {
			console.warn("no projects")
			toast.add({ severity: 'warn', summary: 'No projects found', detail: 'Contact your admin to add project options' })
			return
		}

		projectsRaw.value = allProjects;
		updateFilteredProjects()
	} catch (error) {
		console.error(error)
	} finally {
		loading.value = false
	}
}

const updateFilteredProjects = () => {
	const filterToAllowed = (l: ProjectDto[]) => l.filter(x => !x.requiresNda || willSignContract.value)
	projects.value = projects.value.map(filterToAllowed);

	// Add back previously disallowed projects when re-ticking willSignContract
	if (willSignContract.value) {
		for (const proj of projectsRaw.value) {
			if (!projects.value[0].includes(proj) && !projects.value[1].includes(proj)) {
				projects.value[0].push(proj)
			}
		}
	}
}

const warnIfExceededPreferenceLimit = () => {
	if (projects.value[1].length > maxNumberOfPreferences) {
		if (!exceededPreferenceLimit) {
			toast.add({ severity: 'warn', summary: 'Max preferences reached', detail: warningMessage, life: 30000 });
			exceededPreferenceLimit = true;
		}
	}
	else {
		exceededPreferenceLimit = false;
	}
}

const submitForm = async () => {
	const submitModel: StudentSubmissionDto = {
		files: files.value,
		orderedPreferences: projects.value[1].map(p => p.id).splice(0, maxNumberOfPreferences),
		willSignContract: willSignContract.value,
		classId: classId,
		notes: personalStatement.value,
	}
	const result = await ApiService.post("/students/me", submitModel)
	if (result == null) {
		toast.add({ severity: 'error', summary: 'Failed', detail: 'Submission failed. If the issue persists contact developers', life: 5000 });
	} else {
		showSuccessDialog.value = true;
	}
};

const onUpload = async (event: FileUploadUploaderEvent) => {
	if (Array.isArray(event.files)) { // File[]
		for (let i = 0; i < event.files.length; i++) {
			const formData = new FormData()
			formData.append('file', event.files[i])
			await ApiService.postRaw(`students/file?classId=${classId}`, formData)
		}
	} else { // File
		const formData = new FormData()
		formData.append('file', event.files)
		await ApiService.postRaw(`students/file?classId=${classId}`, formData)
	}

	files.value = await ApiService.get<FileDetailsDto[]>(`/students/files?classId=${classId}`)
}

const deleteFile = async (id: string) => {
	await ApiService.delete(`/students/file/${id}?classId=${classId}`)
	files.value = await ApiService.get<FileDetailsDto[]>(`/students/files?classId=${classId}`)
	toast.add({ severity: 'success', summary: 'Success', detail: 'File deleted successfully', life: 5000 });
}

const not = <Args extends unknown[]>(f: (...args: Args) => boolean) => (...args: Args): boolean => !f(...args);

// Cleanup resize listener on unmount
onUnmounted(() => {
	window.removeEventListener('resize', checkMobile)
})
</script>

<style>
.p-fileupload-file-thumbnail {
	display: none;
}

/* Mobile-specific styles */
@media (max-width: 767px) {
	.mobile-picklist .p-picklist-list {
		min-height: 250px !important;
	}
	
	.mobile-picklist .p-picklist-item {
		padding: 0.5rem;
		font-size: 0.875rem;
	}
	
	.mobile-datatable .p-datatable-wrapper {
		font-size: 0.75rem;
	}
	
	.mobile-datatable .p-datatable-header {
		padding: 0.5rem;
	}
	
	.mobile-datatable .p-datatable-thead > tr > th {
		padding: 0.5rem;
		font-size: 0.75rem;
	}
	
	.mobile-datatable .p-datatable-tbody > tr > td {
		padding: 0.5rem;
		font-size: 0.75rem;
	}
	
	.mobile-fileupload .p-fileupload-content {
		padding: 1rem 0.5rem;
	}
	
	.mobile-fileupload .p-fileupload-buttonbar {
		padding: 0.5rem;
	}
	
	/* Make badges smaller on mobile */
	.p-badge {
		font-size: 0.625rem;
		padding: 0.125rem 0.25rem;
	}
	
	/* Adjust toggle switch for mobile */
	.p-toggleswitch {
		transform: scale(0.9);
	}
}

/* Ensure proper spacing on all screen sizes */
.p-picklist {
	width: 100%;
}

.p-picklist .p-picklist-list {
	border-radius: 0.375rem;
}

.p-message {
	font-size: 0.875rem;
}

/* Improve button touch targets on mobile */
@media (max-width: 767px) {
	.p-button {
		min-height: 44px;
	}
	
	.p-button.p-button-sm {
		min-height: 36px;
	}
}
</style>
