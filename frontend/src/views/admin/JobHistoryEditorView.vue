<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { jobHistoryService, type JobHistory, type JobHistoryDto } from '../../services/jobHistory'
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()
const jobs = ref<JobHistory[]>([])
const loading = ref(true)
const saving = ref(false)
const showForm = ref(false)
const editing = ref<JobHistory | null>(null)

const form = ref<JobHistoryDto>({
  company: '', role: '', description: null, startDate: '', endDate: null, isCurrentRole: false, order: 0,
})

async function loadJobs() {
  loading.value = true
  try { jobs.value = (await jobHistoryService.getAll()).data }
  finally { loading.value = false }
}

onMounted(loadJobs)

function openCreate() {
  editing.value = null
  form.value = { company: '', role: '', description: null, startDate: '', endDate: null, isCurrentRole: false, order: 0 }
  showForm.value = true
}

function openEdit(j: JobHistory) {
  editing.value = j
  form.value = { company: j.company, role: j.role, description: j.description, startDate: j.startDate.slice(0, 10), endDate: j.endDate ? j.endDate.slice(0, 10) : null, isCurrentRole: j.isCurrentRole, order: j.order }
  showForm.value = true
}

async function save() {
  saving.value = true
  try {
    const dto: JobHistoryDto = {
      ...form.value,
      description: form.value.description?.trim() || null,
      startDate: new Date(form.value.startDate).toISOString(),
      endDate: form.value.isCurrentRole ? null : (form.value.endDate ? new Date(form.value.endDate).toISOString() : null),
    }
    if (editing.value) {
      await jobHistoryService.update(editing.value.id, dto)
      toast.success('Job entry updated.')
    } else {
      await jobHistoryService.create(dto)
      toast.success('Job entry created.')
    }
    showForm.value = false
    await loadJobs()
  } finally {
    saving.value = false
  }
}

async function remove(id: number) {
  if (!confirm('Delete this job entry?')) return
  await jobHistoryService.delete(id)
  toast.success('Job entry deleted.')
  await loadJobs()
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short' })
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-mono text-sm font-bold text-slate-900 dark:text-slate-100">job-history/</h1>
      <button @click="openCreate" class="px-3 py-1.5 rounded-lg bg-emerald-500 dark:bg-cyan-500 text-white text-xs font-mono hover:bg-emerald-600 dark:hover:bg-cyan-600 transition-colors">+ new entry</button>
    </div>

    <div v-if="showForm" class="mb-6 border border-slate-200 dark:border-slate-700 rounded-xl p-5 bg-white dark:bg-slate-900 space-y-4">
      <h2 class="font-mono text-xs text-slate-400">{{ editing ? 'edit entry' : 'new entry' }}</h2>
      <div class="grid sm:grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">company</label>
          <input v-model="form.company" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">role</label>
          <input v-model="form.role" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">start date</label>
          <input v-model="form.startDate" type="date" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">end date</label>
          <input v-model="form.endDate" type="date" :disabled="form.isCurrentRole" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400 disabled:opacity-50" />
        </div>
      </div>
      <div>
        <label class="block text-xs font-mono text-slate-500 mb-1">description</label>
        <textarea v-model="form.description" rows="3" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-2 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400 resize-none" />
      </div>
      <div class="flex items-center gap-4">
        <label class="flex items-center gap-2 text-xs font-mono text-slate-600 dark:text-slate-400 cursor-pointer">
          <input v-model="form.isCurrentRole" type="checkbox" class="accent-emerald-500" />
          current role
        </label>
        <div class="flex items-center gap-2">
          <label class="text-xs font-mono text-slate-500">order</label>
          <input v-model.number="form.order" type="number" class="w-20 border border-slate-200 dark:border-slate-700 rounded px-2 py-1 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
        <div class="ml-auto flex gap-2">
          <button @click="showForm = false" class="px-3 py-1.5 text-xs font-mono border border-slate-200 dark:border-slate-700 rounded hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">cancel</button>
          <button @click="save" :disabled="saving" class="px-3 py-1.5 text-xs font-mono rounded bg-emerald-500 dark:bg-cyan-500 text-white hover:bg-emerald-600 dark:hover:bg-cyan-600 transition-colors disabled:opacity-50">{{ saving ? 'saving...' : 'save' }}</button>
        </div>
      </div>
    </div>

    <div v-if="loading" class="text-slate-400 font-mono text-sm animate-pulse">▋ loading...</div>
    <div v-else class="space-y-2">
      <div v-for="j in jobs" :key="j.id" class="flex items-center gap-3 border border-slate-200 dark:border-slate-700 rounded-lg px-4 py-3 bg-white dark:bg-slate-900">
        <div class="flex-1 min-w-0">
          <p class="font-medium text-sm text-slate-900 dark:text-slate-100 truncate">{{ j.role }} <span class="text-emerald-600 dark:text-cyan-400 font-mono">@ {{ j.company }}</span></p>
          <p class="text-xs font-mono text-slate-400">{{ formatDate(j.startDate) }} → {{ j.isCurrentRole ? 'present' : (j.endDate ? formatDate(j.endDate) : '?') }}</p>
        </div>
        <button @click="openEdit(j)" class="text-xs font-mono text-slate-500 hover:text-slate-900 dark:hover:text-slate-100 transition-colors">edit</button>
        <button @click="remove(j.id)" class="text-xs font-mono text-red-400 hover:text-red-600 transition-colors">delete</button>
      </div>
    </div>
  </div>
</template>
