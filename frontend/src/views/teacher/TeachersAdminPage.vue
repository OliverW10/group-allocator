<template>
	<TeacherNavBar :class-id="classId" />
	<div class="px-4 py-2 flex flex-col gap-4 ml-4">
		<h1 class="heading">Manage Teachers</h1>

		<div class="flex gap-2 ml-4">
			<InputText v-model="newTeacherEmail" placeholder="Teacher Email" />
			<Button v-tooltip.top="!hasValidEmail ? 'Enter email to add teacher' : 'Add Teacher'" :loading="isLoading" :disabled="!hasValidEmail" label="Add Teacher" @click="addTeacher" />
		</div>

		<!-- Teachers Table -->
		<div class="w-1/2">
			<DataTable :value="teachers"
				:loading="isLoading"
				empty-message="No teachers found."
				>

				<Column field="email" header="Email" style="white-space: nowrap;">
					<template #body="{ data }">
						<span class="text-gray-600">{{ data.email }}</span>
					</template>
				</Column>

				<Column field="isOwner" header="Role">
					<template #body="{ data }">
						<span>{{ data.isOwner ? 'Owner' : 'Non-Owner' }}</span>
					</template>
				</Column>

				<Column header="Actions" style="width: 100px; white-space: nowrap;">
					<template #body="{ data }">
						<Button v-if="!data.isOwner" v-tooltip.top="!isCurrentTeacherOwner ? 'Only the owner can delete teachers' : 'Delete Teacher'" :loading="isLoading" icon="i-mdi-delete"
							severity="danger" text :disabled="!isCurrentTeacherOwner" @click="deleteTeacher(data.email)" />
					</template>
				</Column>
			</DataTable>
		</div>

		<h1 class="heading">Export/Import</h1>
		
		<div class="flex justify-start gap-3">
			<FileUpload mode="basic" name="file" auto accept=".json" choose-label="Upload JSON"
					@select="uploadBackupJson" />
			<Button label="Download JSON" icon="i-mdi-download" @click="downloadBackupJson" />
			<p v-if="processingUpload">Uploading</p>
		</div>

		<h1 class="heading">Configure Student Form</h1>
		<div v-for="(value, key) of formConfigFlags" :key="key" class="flex">
			<Checkbox v-model="value.value" :name="key + '-checkbox'" binary class="mx-2" :disabled="!value.enabled" :input-id="key+'-checkbox'" />
			<label :for="key + '-checkbox'">{{ value.label }}{{ !value.enabled ? " (Coming Soon)" : "" }}</label>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import ApiService from '../../services/ApiService'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { useRoute } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import TeacherNavBar from '../../components/TeacherNavBar.vue';
import type { TeacherDto } from '../../dtos/teacher-dto'
import { useAuthStore } from '../../store/auth'
import Checkbox from 'primevue/checkbox';
import FileUpload, { type FileUploadSelectEvent } from 'primevue/fileupload'
import { downloadFromUrl } from '../../helpers/download'
import { ClassInfoDto } from '../../dtos/class-info-dto'

const teachers = ref<TeacherDto[]>([])
const newTeacherEmail = ref('')
const isLoading = ref(false)
const formConfigFlags = ref({
	fileUploadEnabled: {
		label: 'Enable File Upload',
		value: false,
		enabled: false,
	},
	freeTextEnabled: {
		label: 'Enable Free Text Input',
		value: false,
		enabled: false,
	},
	ndaEnabled: {
		label: 'Enable NDA Agreement Selection',
		value: false,
		enabled: false,
	},
	friendsEnabled: {
		label: 'Enable Selecting Partner Preferences',
		value: false,
		enabled: false,
	},
});

const toast = useToast();
const route = useRoute();
const classId = route.params.classId as string;
const authStore = useAuthStore()
const isCurrentTeacherOwner = computed(() => {
	return teachers.value.some(t => t.email === authStore.userInfo?.email && t.isOwner)
})
const hasValidEmail = computed(() => {
	return newTeacherEmail.value.trim().includes('@')
})
let classInfo: undefined | ClassInfoDto = undefined; 
const processingUpload = ref(false);

const loadTeachers = async () => {
	isLoading.value = true
	try {
		const response = await ApiService.get<TeacherDto[]>(`/class/${classId}/teachers`)
		if (response) {
			teachers.value = response
		}
		classInfo = await ApiService.get<ClassInfoDto>(`/class/code/${classId}`)
	} catch {
		toast.add({ severity: 'error', summary: 'Failed', detail: `Failed to get teachers`, life: 3000 });
	} finally {
		isLoading.value = false
	}
}

const addTeacher = async () => {
	if (!newTeacherEmail.value.trim()) return

	isLoading.value = true

	try {
		await ApiService.post<string>(`/class/${classId}/add-teacher/${encodeURIComponent(newTeacherEmail.value.trim())}`, {})

		teachers.value.push({ email: newTeacherEmail.value.trim(), isOwner: false })
		newTeacherEmail.value = ''
		toast.add({ severity: 'success', summary: 'Success', detail: `Teacher ${newTeacherEmail.value.trim()} added successfully`, life: 3000 });
	} catch {
		toast.add({ severity: 'error', summary: 'Failed', detail: `Failed to add teacher`, life: 3000 });
	} finally {
		isLoading.value = false
	}
}

const deleteTeacher = async (teacherEmail: string) => {
	if (!confirm('Are you sure you want to remove this teacher?')) return

	isLoading.value = true

	try {
		await ApiService.delete(`/class/${classId}/remove-teacher/${encodeURIComponent(teacherEmail)}`)
		teachers.value = teachers.value.filter(t => t.email !== teacherEmail)
		toast.add({ severity: 'success', summary: 'Success', detail: `Teacher ${teacherEmail} removed successfully`, life: 3000 });
	} catch {
		toast.add({ severity: 'error', summary: 'Failed', detail: `Failed to remove teacher ${teacherEmail}`, life: 3000 });
	} finally {
		isLoading.value = false
	}
}

const downloadBackupJson = () => {
	const dateString = new Date().toISOString().split('T')[0];
	downloadFromUrl(`/class/${classId}/download`, `class_backup_${classInfo?.name}_${dateString}.json`)
}

const uploadBackupJson = async (event: FileUploadSelectEvent) => {
	processingUpload.value = true;
	try {
		const file = event.files[0];
		if (!file) {
			toast.add({ severity: 'error', summary: 'No file selected', life: 3000 });
			processingUpload.value = false;
			return;
		}
		const reader = new FileReader();
		reader.onload = async (e) => {
			try {
				const json = JSON.parse(e.target?.result as string);
				// Optionally validate structure here
				await ApiService.post(`/class/${classId}/import`, json);
				toast.add({ severity: 'success', summary: 'Import successful', life: 3000 });
			} catch {
				toast.add({ severity: 'error', summary: 'Invalid JSON or import failed', life: 3000 });
			} finally {
				processingUpload.value = false;
			}
		};
		reader.onerror = () => {
			toast.add({ severity: 'error', summary: 'File read error', life: 3000 });
			processingUpload.value = false;
		};
		reader.readAsText(file);
	} catch {
		toast.add({ severity: 'error', summary: 'Unexpected error', life: 3000 });
		processingUpload.value = false;
	}
}

onMounted(() => {
	loadTeachers()
})
</script>
