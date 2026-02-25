<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { projectsService, type Project, type ProjectDto } from '../../services/projects'
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()
const projects = ref<Project[]>([])
const loading = ref(true)
const saving = ref(false)
const showForm = ref(false)
const editing = ref<Project | null>(null)

const form = ref<ProjectDto>({
  title: '', description: '', techStack: null, liveUrl: null, githubUrl: null, imageUrl: null, order: 0,
})

async function loadProjects() {
  loading.value = true
  try { projects.value = (await projectsService.getAll()).data }
  finally { loading.value = false }
}

onMounted(loadProjects)

function openCreate() {
  editing.value = null
  form.value = { title: '', description: '', techStack: null, liveUrl: null, githubUrl: null, imageUrl: null, order: 0 }
  showForm.value = true
}

function openEdit(p: Project) {
  editing.value = p
  form.value = { title: p.title, description: p.description, techStack: p.techStack, liveUrl: p.liveUrl, githubUrl: p.githubUrl, imageUrl: p.imageUrl, order: p.order }
  showForm.value = true
}

async function save() {
  saving.value = true
  try {
    const dto: ProjectDto = {
      ...form.value,
      techStack: form.value.techStack?.trim() || null,
      liveUrl: form.value.liveUrl?.trim() || null,
      githubUrl: form.value.githubUrl?.trim() || null,
      imageUrl: form.value.imageUrl?.trim() || null,
    }
    if (editing.value) {
      await projectsService.update(editing.value.id, dto)
      toast.success('Project updated.')
    } else {
      await projectsService.create(dto)
      toast.success('Project created.')
    }
    showForm.value = false
    await loadProjects()
  } finally {
    saving.value = false
  }
}

async function remove(id: number) {
  if (!confirm('Delete this project?')) return
  await projectsService.delete(id)
  toast.success('Project deleted.')
  await loadProjects()
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-mono text-sm font-bold text-slate-900 dark:text-slate-100">projects/</h1>
      <button @click="openCreate" class="px-3 py-1.5 rounded-lg bg-emerald-500 dark:bg-cyan-500 text-white text-xs font-mono hover:bg-emerald-600 dark:hover:bg-cyan-600 transition-colors">+ new project</button>
    </div>

    <div v-if="showForm" class="mb-6 border border-slate-200 dark:border-slate-700 rounded-xl p-5 bg-white dark:bg-slate-900 space-y-4">
      <h2 class="font-mono text-xs text-slate-400">{{ editing ? 'edit project' : 'new project' }}</h2>
      <div class="grid sm:grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">title</label>
          <input v-model="form.title" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">tech stack (comma-separated)</label>
          <input v-model="form.techStack" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
      </div>
      <div>
        <label class="block text-xs font-mono text-slate-500 mb-1">description</label>
        <textarea v-model="form.description" rows="3" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-2 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400 resize-none" />
      </div>
      <div class="grid sm:grid-cols-3 gap-4">
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">live url</label>
          <input v-model="form.liveUrl" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">github url</label>
          <input v-model="form.githubUrl" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">order</label>
          <input v-model.number="form.order" type="number" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
      </div>
      <div>
        <label class="block text-xs font-mono text-slate-500 mb-1">image url</label>
        <input v-model="form.imageUrl" placeholder="https://..." class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
      </div>
      <div class="flex justify-end gap-2">
        <button @click="showForm = false" class="px-3 py-1.5 text-xs font-mono border border-slate-200 dark:border-slate-700 rounded hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">cancel</button>
        <button @click="save" :disabled="saving" class="px-3 py-1.5 text-xs font-mono rounded bg-emerald-500 dark:bg-cyan-500 text-white hover:bg-emerald-600 dark:hover:bg-cyan-600 transition-colors disabled:opacity-50">{{ saving ? 'saving...' : 'save' }}</button>
      </div>
    </div>

    <div v-if="loading" class="text-slate-400 font-mono text-sm animate-pulse">▋ loading...</div>
    <div v-else class="space-y-2">
      <div v-for="p in projects" :key="p.id" class="flex items-center gap-3 border border-slate-200 dark:border-slate-700 rounded-lg px-4 py-3 bg-white dark:bg-slate-900">
        <span class="flex-1 font-medium text-sm text-slate-900 dark:text-slate-100 truncate">{{ p.title }}</span>
        <span class="text-xs font-mono text-slate-400">order: {{ p.order }}</span>
        <button @click="openEdit(p)" class="text-xs font-mono text-slate-500 hover:text-slate-900 dark:hover:text-slate-100 transition-colors">edit</button>
        <button @click="remove(p.id)" class="text-xs font-mono text-red-400 hover:text-red-600 transition-colors">delete</button>
      </div>
    </div>
  </div>
</template>
