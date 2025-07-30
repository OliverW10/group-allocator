<template>
  <div class="p-8">
    <div class="flex justify-between items-center mb-8">
      <h1 class="text-2xl font-bold">My Classes</h1>
      <div class="flex items-center gap-4">
        <Button 
          label="Create New Class"
          @click="showCreateModal = true"
        />
        <Button 
          v-if="authStore.userInfo?.isAdmin"
          label="Manage Teachers"
          @click="navigateToTeachers"
          severity="secondary"
        />
        <LogoutButton />
      </div>
    </div>

    <!-- Classes Table -->
    <DataTable 
      :value="classes" 
      :paginator="true" 
      :rows="10"
      :rows-per-page-options="[5, 10, 20]"
      table-style="min-width: 50rem"
      class="p-datatable-sm"
      selection-mode="single"
      @row-click="(event) => navigateToSolver(event.data.id)"
    >
      <Column field="name" header="Class Name">
        <template #body="{ data }">
          <span v-if="!editingClass || editingClass.id !== data.id">
            {{ data.name }}
          </span>
          <InputText
            v-else
            ref="editInput"
            v-model="editingClass.name"
            class="w-full"
            @blur="saveClassEdit"
            @keyup.enter="saveClassEdit"
          />
          <Button
              v-if="!editingClass || editingClass.id !== data.id"
              icon="i-mdi-pencil"
              severity="info"
              text
              @click.stop="startEditing(data)"
            />
        </template>
      </Column>
      <Column field="code" header="Class Code">
        <template #body="{ data }">
          <div class="flex items-center gap-2">
            <span class="font-mono" @click.stop="navigateToCode(data.id)">
              <Button :label="data.code" icon="i-mdi-eye" variant="text" />
            </span>
          </div>
        </template>
      </Column>
      <Column field="studentCount" header="Number of Students" />
      <Column field="createdAt" header="Created At">
        <template #body="{ data }">
          {{ formatRelativeDate(data.createdAt) }}
        </template>
      </Column>
      <Column header="Plan">
        <template #body="{ data }">
          <div class="flex items-center gap-2">
            {{ data.payed ? 'Payed' : 'Free' }}
            <Button
              v-if="!data.payed"
              v-tooltip.top="'Free plan classes allow up to 20 students'"
              icon="i-mdi-information"
              severity="info" 
              text
              @click.stop
            />
            <Button
              v-if="!data.payed && data.studentCount > 20"
              v-tooltip.top="'Student limit exceeded. Please upgrade your plan for this class'"
              icon="i-mdi-alert"
              severity="warning"
              text
              @click.stop
            />
            <Button
              v-if="!data.payed"
              v-tooltip.top="'Upgrade to Premium'"
              icon="i-mdi-shopping"
              severity="success"
              text
              @click.stop="showPurchaseConfirmation(data)"
            />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center p-4 text-gray-500">No classes yet</div>
      </template>
    </DataTable>

    <!-- Create Class Modal -->
    <Dialog
      v-model:visible="showCreateModal"
      modal
      header="Create New Class"
      :style="{ width: '30rem' }"
    >
      <form class="flex flex-col gap-4" @submit.prevent="createClass">
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

    <!-- Purchase Confirmation Dialog -->
    <Dialog
      v-model:visible="showPurchaseModal"
      modal
      header="Confirm Purchase"
      :closable="!isPurchaseLoading"
      :style="{ width: '50rem' }"
    >
      <div class="p-4">
        <p class="mb-4">Are you sure you want to upgrade <strong>{{ selectedClass?.name }}</strong> to a paid plan for $5 AUD?</p>
        <p class="mb-4">This will allow you to run the automatic group allocation for an unlimited number of students for this class, the paid plan for this class will expire 6 months from now on {{ moment().add(6, 'months').format('MMMM Do, YYYY') }}.</p>
        <p class="text-sm text-gray-600 mb-4">This will redirect you to Stripe to complete the payment.</p>
      </div>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button
            @click="showPurchaseModal = false"
            label="Cancel"
            severity="secondary"
            text
            :disabled="isPurchaseLoading"
          />
          <Button
            @click="handlePurchase"
            label="Proceed to Purchase"
            icon="i-mdi-check"
            :loading="isPurchaseLoading"
          />
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import moment from 'moment'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import InputText from 'primevue/inputtext'
import Dialog from 'primevue/dialog'
import ApiService from '../../services/ApiService'
import LogoutButton from '../../components/LogoutButton.vue'
import { ClassResponseDto } from '../../dtos/class-response-dto'
import { useAuthStore } from '../../store/auth'

const router = useRouter()
const authStore = useAuthStore()
const classes = ref<ClassResponseDto[]>([])
const showCreateModal = ref(false)
const newClass = ref({ name: '' })
const editingClass = ref<ClassResponseDto | null>(null)
const showPurchaseModal = ref(false)
const selectedClass = ref<ClassResponseDto | null>(null)
const isPurchaseLoading = ref(false)

const fetchClasses = async () => {
  try {
    classes.value = await ApiService.get<ClassResponseDto[]>(
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

const startEditing = (classItem: ClassResponseDto) => {
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

const navigateToSolver = (classId: number) => {
  router.push(`/teacher/${classId}/projects/`)
}

const navigateToCode = (classId: number) => {
  router.push(`/teacher/${classId}/code/`)
}

const navigateToTeachers = () => {
  router.push('/teachers')
}

const formatRelativeDate = (date: Date) => {
  return moment(date).fromNow()
}

const showPurchaseConfirmation = (classItem: ClassResponseDto) => {
  selectedClass.value = classItem
  showPurchaseModal.value = true
}

const handlePurchase = async () => {
  if (!selectedClass.value || isPurchaseLoading.value) return
  
  isPurchaseLoading.value = true
  try {
    // Get the current frontend URL for the return domain
    const returnDomain = window.location.origin + `/teacher/${selectedClass.value.id}/purchase`
    
    // Make the GET request to create Stripe session
    const stripeUrl = await ApiService.get<string>(`/payment/create-stripe-session?returnDomain=${encodeURIComponent(returnDomain)}`)
    
    if (stripeUrl) {
      // Redirect to the Stripe checkout URL
      window.location.href = stripeUrl
    } else {
      throw new Error('Failed to create Stripe session')
    }
  } catch (error) {
    console.error('Purchase failed:', error)
    alert('Failed to initiate purchase. Please try again.')
  } finally {
    isPurchaseLoading.value = false
    showPurchaseModal.value = false
  }
}

onMounted(fetchClasses)
</script>
