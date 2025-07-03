<template>
    <AdminNavBar :class-id="classId" />
	<Dialog :visible="fileModal != null" :modal="true" :closable="false" :draggable="false" :header="'Files for ' + students.find((student) => student.studentInfo.studentId == fileModal)?.studentInfo.name" :style="{ width: '50vw' }">
		<DataTable :value="students.find((student) => student.studentInfo.studentId == fileModal)?.studentSubmission.files" :loading="loading" :paginator="true" :rows="10" :rows-per-page-options="[5, 10, 20, 50]">
			<Column field="name" header="Name"></Column>
			<Column field="actions" header="Actions">
				<template #body="slotProps">
					<Button severity="info" class="i-mdi-download" @click="download(slotProps.data.studentInfo.studentId, slotProps.data.studentInfo.name)" />
					<Button severity="danger" class="i-mdi-delete" @click="deleteFile(slotProps.data.studentInfo.studentId)" />
				</template>
			</Column>
		</DataTable>
		<Button label="Done" severity="secondary" class="mt-4" @click="fileModal = null" />
	</Dialog>
	<Dialog :visible="addStudentModal" :modal="true" :closable="false" :draggable="false" header="Add Student" :style="{ width: '30vw' }">
		<div class="flex flex-col gap-4">
			<div class="flex flex-col gap-2">
				<label for="email" class="font-medium">Email *</label>
				<InputText id="email" v-model="newStudent" placeholder="student@example.com" />
			</div>
			<div class="flex gap-2 justify-end">
				<Button label="Cancel" severity="secondary" @click="addStudentModal = false" />
				<Button label="Add Student" severity="success" @click="addStudent" :loading="addingStudent" />
			</div>
		</div>
	</Dialog>
    <div class="px-4 py-2 mt-4 flex flex-col gap-4">
        <h1 class="heading">Students</h1>
        <Divider style="margin: 0;" />
        <div class="flex gap-4 items-center justify-between">
            <FileUploader label="Upload Students List" @projects-changed="uploadStudents">
                <p>
                    You can upload a list of students to allow them to join the class without a code or to allocate them to a project group without them submitting preferences.
                    <br />
                    Please upload a text file with a single student email on each line.
                    <br />
                    <br />
                    Student rows highlighted in yellow have not submitted any preferences yet, students highlighted in green have submitted preferences and were part of the uploaded students list, students highlighted in red have submitted preferences but were not part of the uploaded students list.
                </p>
            </FileUploader>
            <Button label="Add Student" icon="i-mdi-plus" @click="addStudentModal = true" />
        </div>
        <DataTable :value="students" :loading="loading" :paginator="true" :rows="10" :rows-per-page-options="[5, 10, 20, 50]" :row-class="rowClass">
            <Column field="studentInfo.name" header="Name"></Column>
            <Column field="studentInfo.email" header="Email"></Column>
            <Column field="willSignContract" header="NDA?">
                <template #body="slotProps">
                    {{ slotProps.data.studentSubmission.willSignContract ? '✔️' : '❌' }}
                </template>
            </Column>
            <Column field="orderedPreferences" header="Preferences">
                <template #body="slotProps">
                    {{slotProps.data.studentSubmission.orderedPreferences.map((projId: number) => projects.find((proj: ProjectDto) =>
                        projId == proj.id)?.name).join(', ')}}
                </template>
            </Column>
            <Column field="actions" header="Actions">
                <template #body="slotProps">
					<div class="flex">
						<Button severity="info" class="i-mdi-files" @click="showFiles(slotProps.data.studentInfo.studentId)" />
						<Button severity="danger" class="i-mdi-delete" @click="remove(slotProps.data.studentInfo.studentId)" />
					</div>
                </template>
            </Column>
            <template #empty>
                <div class="text-center p-4 text-gray-500">
                  No students yet, either share the class code with students or upload a list of students.
                </div>
            </template>
        </DataTable>
    </div>
</template>
<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import AdminNavBar from '../../components/TeacherNavBar.vue';
import ApiService from '../../services/ApiService';
import DataTable from 'primevue/datatable';
import Button from 'primevue/button';
import Column from 'primevue/column';
import Divider from 'primevue/divider';
import InputText from 'primevue/inputtext';
import { Dialog, useToast, type FileUploadSelectEvent } from 'primevue';
import { ProjectDto } from '../../dtos/project-dto';
import FileUploader from '../../components/FileUploader.vue';
import type { StudentInfoAndSubmission } from '../../dtos/student-info-and-submission';

const students = ref([] as StudentInfoAndSubmission[]);
const projects = ref([] as ProjectDto[])
const loading = ref(false);
const toast = useToast();
const fileModal = ref(null as null | number);
const addStudentModal = ref(false);
const addingStudent = ref(false);
const newStudent = ref('');
const route = useRoute();
const classId = route.params.classId as string;

const showFiles = (id: number) => {
	fileModal.value = id;
}

const rowClass = (data: StudentInfoAndSubmission) => {
    return [{ '!bg-green500/20': data.studentInfo.isVerified && data.studentSubmission.orderedPreferences.length > 0,
    '!bg-yellow500/20': data.studentInfo.isVerified && data.studentSubmission.orderedPreferences.length == 0,
    '!bg-red500/20': !data.studentInfo.isVerified }]
};

onMounted(async () => {
    try {
        loading.value = true;
        students.value = await ApiService.get<StudentInfoAndSubmission[]>(`/students?classId=${classId}`)
        projects.value = await ApiService.get<ProjectDto[]>(`/projects?classId=${classId}`)
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
});

const deleteFile = async (id: string) => {
	await ApiService.delete(`/students/file/${id}?classId=${classId}`)
	students.value = await ApiService.get<StudentInfoAndSubmission[]>(`/students?classId=${classId}`)
}

const download = async (id: unknown, name: string) => {
	const url = await ApiService.makeUrl(`/students/file/${id}?classId=${classId}`)
	const a = document.createElement('a')
	a.href = url.toString()
	a.download = name
	document.body.appendChild(a)
	a.click()
	document.body.removeChild(a)
}

const setStudents = (data: StudentInfoAndSubmission[]) => {
    students.value = data;
}

const remove = async (id: string) => {
    const newProjects = await ApiService.delete<StudentInfoAndSubmission[]>(`/students/${id}?classId=${classId}`);
    setStudents(newProjects);
}

const uploadStudents = async (event: FileUploadSelectEvent) => {
    if (!event.files) {
        console.error('selected no file, ignoring')
    }
    const selectedFile = event.files[0]
    const formData = new FormData()
    formData.append('file', selectedFile)
    try {
        const result = await ApiService.postRaw<StudentInfoAndSubmission[]>(`students/whitelist?classId=${classId}`, formData)
        setStudents(result)
        if (result != undefined) {
            toast.add({ severity: 'success', summary: 'Success', detail: 'Students added to whitelist', life: 5000 });
        } else {
            error();
        }
    } catch {
        error();
    }
}

const error = () => {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Students whitelist file upload failed', life: 10000 })
}

const addStudent = async () => {
    const email = newStudent.value.trim();
    if (!email) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Email is required', life: 5000 });
        return;
    }

    try {
        addingStudent.value = true;
        const result = await ApiService.post<StudentInfoAndSubmission[]>(`students/add?classId=${classId}&email=${email}`, {});
        setStudents(result);
        addStudentModal.value = false;
        newStudent.value = '';
        toast.add({ severity: 'success', summary: 'Success', detail: 'Student added successfully', life: 5000 });
    } catch (error) {
        console.error(error);
        toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to add student', life: 10000 });
    } finally {
        addingStudent.value = false;
    }
}
</script>
