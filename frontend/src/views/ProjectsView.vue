<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { projectsService, type Project } from '../services/projects'

const projects = ref<Project[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await projectsService.getAll()
    projects.value = res.data
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="pb-16">
    <div class="mb-8 flex items-center gap-3">
      <div class="w-2 h-2 rounded-full bg-emerald-500 dark:bg-cyan-400" />
      <h1 class="font-mono text-xs uppercase tracking-widest text-slate-400 dark:text-slate-500">projects/</h1>
    </div>

    <div v-if="loading" class="text-slate-400 font-mono text-sm animate-pulse">▋ loading...</div>

    <div v-else-if="!projects.length" class="text-slate-400 font-mono text-sm">// no projects yet.</div>

    <div v-else class="grid sm:grid-cols-2 gap-4">
      <div
        v-for="project in projects"
        :key="project.id"
        class="group border border-slate-200 dark:border-slate-700 rounded-lg p-5 hover:border-emerald-300 dark:hover:border-cyan-700 transition-all hover:shadow-sm"
      >
        <img v-if="project.imageUrl" :src="project.imageUrl" :alt="project.title" class="w-full h-32 object-cover rounded mb-3" />
        <h2 class="font-semibold text-slate-900 dark:text-slate-100 group-hover:text-emerald-600 dark:group-hover:text-cyan-400 transition-colors">{{ project.title }}</h2>
        <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">{{ project.description }}</p>
        <div class="mt-3 flex flex-wrap gap-1">
          <span
            v-for="tech in (project.techStack || '').split(',').filter(Boolean)"
            :key="tech"
            class="text-xs font-mono bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 rounded px-1.5 py-0.5"
          >{{ tech.trim() }}</span>
        </div>
        <div class="mt-3 flex gap-3">
          <a v-if="project.liveUrl" :href="project.liveUrl" target="_blank" class="text-xs font-mono text-emerald-600 dark:text-cyan-400 hover:underline">live ↗</a>
          <a v-if="project.githubUrl" :href="project.githubUrl" target="_blank" class="text-xs font-mono text-slate-500 dark:text-slate-400 hover:underline">github ↗</a>
        </div>
      </div>
    </div>
  </div>
</template>
