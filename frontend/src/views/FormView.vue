<template>
	<div class="flex flex-col gap-4 p-4 justify-center items-center">
		<h1 class="heading text-center mb-4">Submit Preferences</h1>
		<Card class="min-w-5xl">
			<template #content>
				<div class="p-4">
					<h2 class="text-xl mb-3">Select Your Project Preferences</h2>

					<ProgressBar v-if="loading" mode="indeterminate" style="height: 6px" />

					<PickList v-if="!loading" v-model:model-value="projects" list-style="min-height: 500px" data-key="id" dragdrop :meta-key-selection="true" :show-source-controls="false" v-on:update:model-value="warnIfExceededPreferenceLimit">
						<template #sourceheader><b class="text-lg">Available Projects</b></template>
						<template #targetheader><b class="text-lg">Ordered Preferences</b></template>

						<template #option="{ option }">
							{{ option.name }}
							<Badge v-if="option?.requiresNda" severity="info" class="ml-2" icon="i-mdi-shield-account">
								NDA
							</Badge>
							<Badge v-tooltip="'Client: ' + option.client + '\nMin Students: ' + option.minStudents + '\nMax Students: ' + option.maxStudents" 
								class="ml-auto" size="small" value="?" />
						</template>
					</PickList>
					<div v-if="!willSignContract && someProjectsRequireAnNda" class="flex gap-2 my-4">
						<Message severity="info" icon="i-mdi-shield-account" class="w-full">
							Some projects were filtered out because they require an NDA and you are have not selected to sign one.
						</Message>
					</div>

					<div class="flex gap-2 my-4">
						<label for="switch1">I am willing to sign an NDA to work on a project</label>
						<ToggleSwitch v-model="willSignContract" input-id="switch1" @update:model-value="updateFilteredProjects" />
					</div>

					<FileUpload name="demo[]" url="/api/upload" :multiple="true" :max-file-size="10000000"
						custom-upload @uploader="onUpload($event)">
						<template #empty>
							<span>Drag and drop files to here to upload.</span>
						</template>
					</FileUpload>

					<DataTable :value="files" :paginator="true" :rows="10" class="my-4">
						<Column field="name" header="Name"></Column>
						<Column field="actions" header="Actions">
							<template #body="slotProps">
								<Button severity="danger" class="i-mdi-delete" @click="deleteFile(slotProps.data.id)" />
							</template>
						</Column>
					</DataTable>

					<Button label="Save Preferences" class="mt-4" icon="i-mdi-upload" @click="submitForm" />
				</div>
			</template>
		</Card>
		<LogoutButton />
	</div>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import Badge from "primevue/badge";
import Card from "primevue/card";
import Message from "primevue/message"
import ToggleSwitch from "primevue/toggleswitch";
import FileUpload, { type FileUploadUploaderEvent } from "primevue/fileupload";
import { ProjectDto } from "../dtos/project-dto";
import LogoutButton from "../components/LogoutButton.vue";
import { computed, onMounted, ref } from 'vue'
import PickList from 'primevue/picklist'
import ProgressBar from 'primevue/progressbar'
import ApiService from "../services/ApiService";
import { useToast } from "primevue/usetoast";
import type { FileDetailsDto } from "../dtos/file-details-dto";
import type { StudentSubmissionDto } from "../dtos/student-submission-dto";
import { Column, DataTable } from "primevue";

const toast = useToast();

const files = ref([] as FileDetailsDto[])
const willSignContract = ref(true)
const maxNumberOfPreferences = 10;
const warningMessage = "A maximum of " + maxNumberOfPreferences + " preferences has been selected. Anything more than the top " + maxNumberOfPreferences + " preferences will not be saved";
let exceededPreferenceLimit = false
const loading = ref(false)

const projectsRaw = ref([] as ProjectDto[]);
const projects = ref([[], []] as ProjectDto[][]);
const someProjectsRequireAnNda = computed(() => projectsRaw.value.some(x => x.requiresNda))

onMounted(async () => {
	loading.value = true
	try{
		await loadProjects()
		const maybeStudent = await ApiService.get<StudentSubmissionDto | undefined>("/students/me")
		if (maybeStudent) {
			console.log(maybeStudent)
			files.value = maybeStudent.files ?? []
			willSignContract.value = maybeStudent.willSignContract ?? true
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

		const allProjects = await ApiService.get<ProjectDto[]>("/projects")
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
	}
	const result = await ApiService.post("/students/me", submitModel)
	if (result == null) {
		toast.add({ severity: 'error', summary: 'Failed', detail: 'Submission failed. If the issue persists contact developers', life: 5000 });
	} else {
		toast.add({ severity: 'success', summary: 'Success', detail: 'Submitted preferences', life: 5000 });
		window.location.reload()
	}
};

const onUpload = async (event: FileUploadUploaderEvent) => {
	if (Array.isArray(event.files)) { // File[]
		for (let i = 0; i < event.files.length; i++) {
			const formData = new FormData()
			formData.append('file', event.files[i])
			await ApiService.postRaw('students/file', formData)
		}
	} else { // File
		const formData = new FormData()
		formData.append('file', event.files)
		await ApiService.postRaw('students/file', formData)
	}

	files.value = await ApiService.get<FileDetailsDto[]>('/students/files')
}

const deleteFile = async (id: string) => {
	await ApiService.delete(`/students/file/${id}`)
	files.value = await ApiService.get<FileDetailsDto[]>('/students/files')
	toast.add({ severity: 'success', summary: 'Success', detail: 'File deleted successfully', life: 5000 });
}
const not = <Args extends unknown[]>(f: (...args: Args) => boolean) => (...args: Args): boolean => !f(...args);
</script>

<style>
.p-fileupload-file-thumbnail {
	display: none;
}
</style>
