<template>
	<TeacherNavBar :class-id="classId" />
	<div class="px-4 py-2 mt-4 flex flex-col gap-4 ml-4">
        <div class="flex justify-between items-center">
            <h1 class="heading">Teachers</h1>
        </div>

		<div class="flex gap-2 ml-4">
			<InputText v-model="newTeacherEmail" placeholder="Teacher Email" />
			<Button @click="addTeacher" :loading="isLoading" :disabled="!hasValidEmail" label="Add Teacher" v-tooltip.top="!hasValidEmail ? 'Enter email to add teacher' : 'Add Teacher'" />
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
						<Button v-if="!data.isOwner" @click="deleteTeacher(data.email)" :loading="isLoading" icon="i-mdi-delete"
							severity="danger" text v-tooltip.top="!isCurrentTeacherOwner ? 'Only the owner can delete teachers' : 'Delete Teacher'" :disabled="!isCurrentTeacherOwner" />
					</template>
				</Column>
			</DataTable>
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

const teachers = ref<TeacherDto[]>([])
const newTeacherEmail = ref('')
const isLoading = ref(false)

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

const loadTeachers = async () => {
	isLoading.value = true
	try {
		const response = await ApiService.get<TeacherDto[]>(`/class/${classId}/teachers`)
		if (response) {
			teachers.value = response
		}
	} catch (error) {
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
	} catch (error) {
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
	} catch (error) {
		toast.add({ severity: 'error', summary: 'Failed', detail: `Failed to remove teacher ${teacherEmail}`, life: 3000 });
	} finally {
		isLoading.value = false
	}
}

onMounted(() => {
	loadTeachers()
})
</script>
