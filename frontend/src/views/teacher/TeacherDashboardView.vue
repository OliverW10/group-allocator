<template>
  <div class="p-8">
    <div class="flex justify-between items-center mb-8">
      <h1 class="text-2xl font-bold">My Classes</h1>
      <div class="flex items-center gap-4">
        <Button 
          icon="pi pi-plus"
          label="Create New Class"
          @click="showCreateModal = true"
        />
        <LogoutButton />
      </div>
    </div>

    <!-- Classes Table -->
    <DataTable 
      :value="classes" 
      :paginator="true" 
      :rows="10"
      :rowsPerPageOptions="[5, 10, 20]"
      tableStyle="min-width: 50rem"
      class="p-datatable-sm"
      @row-click="(event) => navigateToSolver(event.data.id)"
      selectionMode="single"
    >
      <Column field="name" header="Class Name">
        <template #body="{ data }">
          <span v-if="!editingClass || editingClass.id !== data.id">
            {{ data.name }}
          </span>
          <InputText
            v-else
            v-model="editingClass.name"
            @blur="saveClassEdit"
            @keyup.enter="saveClassEdit"
            ref="editInput"
            class="w-full"
          />
        </template>
      </Column>
      <Column field="studentCount" header="Number of Students" />
      <Column header="Actions">
        <template #body="{ data }">
          <div class="flex gap-2">
            <Button
              icon="pi pi-pencil"
              severity="info"
              text
              @click.stop="startEditing(data)"
              v-if="!editingClass || editingClass.id !== data.id"
            />
            <Button
              icon="pi pi-trash"
              severity="danger"
              text
              @click.stop="deleteClass(data.id)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <!-- Create Class Modal -->
    <Dialog
      v-model:visible="showCreateModal"
      modal
      header="Create New Class"
      :style="{ width: '30rem' }"
    >
      <form @submit.prevent="createClass" class="flex flex-col gap-4">
        <div class="field">
          <label for="className" class="block mb-2">Class Name:</label>
          <InputText
            id="className"
            v-model="newClass.name"
            required
            placeholder="Enter class name"
            class="w-full"
          />
        </div>
      </form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button
            label="Cancel"
            severity="secondary"
            text
            @click="showCreateModal = false"
          />
          <Button
            label="Create"
            @click="createClass"
          />
        </div>
      </template>
    </Dialog>

    <!-- Delete Confirmation Dialog -->
    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import InputText from 'primevue/inputtext'
import Dialog from 'primevue/dialog'
import ConfirmDialog from 'primevue/confirmdialog'
import ApiService from '../../services/ApiService'
import LogoutButton from '../../components/LogoutButton.vue'

interface Class {
  id: number
  code: string
  name: string
  createdAt: string
  teacherRole: string | null
}

const router = useRouter()
const confirm = useConfirm()
const classes = ref<Class[]>([])
const showCreateModal = ref(false)
const newClass = ref({ name: '' })
const editingClass = ref<Class | null>(null)

const fetchClasses = async () => {
  try {
    classes.value = await ApiService.get<Class[]>(
      '/Class/list-teacher'
    )
  } catch (error) {
    console.error('Error fetching classes:', error)
  }
}

const createClass = async () => {
  try {
    await ApiService.post('/Class', newClass.value)
    showCreateModal.value = false
    newClass.value = { name: '' }
    await fetchClasses()
  } catch (error) {
    console.error('Error creating class:', error)
  }
}

const startEditing = (classItem: Class) => {
  editingClass.value = { ...classItem }
}

const saveClassEdit = async () => {
  if (!editingClass.value) return
  try {
    await ApiService.put(`/Class/${editingClass.value.id}`, {
      name: editingClass.value.name
    })
    await fetchClasses()
    editingClass.value = null
  } catch (error) {
    console.error('Error updating class:', error)
  }
}

const deleteClass = (classId: number) => {
  confirm.require({
    message: 'Are you sure you want to delete this class?',
    header: 'Delete Confirmation',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    accept: async () => {
      try {
        await ApiService.delete(`/Class/${classId}`)
        await fetchClasses()
      } catch (error) {
        console.error('Error deleting class:', error)
      }
    }
  })
}

const navigateToSolver = (classId: number) => {
  router.push(`/teacher/solver/${classId}`)
}

onMounted(fetchClasses)
</script>
