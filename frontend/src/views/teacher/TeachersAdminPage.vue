<template>
	<TeacherNavBar :class-id="classId" />
	<div class="px-4 py-2 mt-4 flex flex-col gap-4 ml-4">
        <div class="flex justify-between items-center">
            <h1 class="heading">Teachers</h1>
        </div>

		<div class="flex gap-2 ml-4">
			<InputText v-model="newTeacherEmail" placeholder="Teacher Email" />
			<Button @click="addTeacher" :loading="isLoading" :disabled="!newTeacherEmail.trim().includes('@')" label="Add Teacher" v-tooltip.top="'Add Teacher'" />
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

				<!-- <Column field="isAdmin" header="Admin">
					<template #body="{ data }">
						<Tag :value="data.isAdmin ? 'Yes' : 'No'" :severity="data.isAdmin ? 'success' : 'secondary'" />
					</template>
				</Column> -->

				<Column header="Actions" style="width: 100px; white-space: nowrap;">
					<template #body="{ data }">
						<Button @click="deleteTeacher(data)" :loading="isLoading" icon="i-mdi-delete"
							severity="danger" text v-tooltip.top="'Delete Teacher'" />
					</template>
				</Column>
			</DataTable>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import ApiService from '../../services/ApiService'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { useRoute } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import TeacherNavBar from '../../components/TeacherNavBar.vue';

interface Teacher {
	email: string;
	isAdmin: boolean;
}

const teachers = ref<Teacher[]>([])
const newTeacherEmail = ref('')
const isLoading = ref(false)

const toast = useToast();
const route = useRoute();
const classId = route.params.classId as string;


const loadTeachers = async () => {
	isLoading.value = true
	try {
		const response = await ApiService.get<string[]>(`/class/${classId}/teachers`)
		if (response) {
			teachers.value = response.map(email => ({ email, isAdmin: false }))
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

		teachers.value.push({ email: newTeacherEmail.value.trim(), isAdmin: false })
		newTeacherEmail.value = ''
		toast.add({ severity: 'success', summary: 'Success', detail: 'Got previous result', life: 1000 });
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
