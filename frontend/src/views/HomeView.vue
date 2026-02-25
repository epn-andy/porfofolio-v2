<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { jobHistoryService, type JobHistory } from '../services/jobHistory'
import { projectsService, type Project } from '../services/projects'

const jobs = ref<JobHistory[]>([])
const projects = ref<Project[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const [j, p] = await Promise.all([jobHistoryService.getAll(), projectsService.getAll()])
    jobs.value = j.data
    projects.value = p.data
  } finally {
    loading.value = false
  }
})

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short' })
}
</script>

<template>
  <div class="space-y-16 pb-16">
    <!-- Hero -->
    <section class="relative pt-12 pb-8">
      <div class="flex items-center gap-3 mb-4">
        <div class="w-3 h-3 rounded-full bg-emerald-500 dark:bg-cyan-400 ring-4 ring-emerald-500/20 dark:ring-cyan-400/20" />
        <span class="font-mono text-xs text-emerald-600 dark:text-cyan-400 uppercase tracking-widest">pipeline: active</span>
      </div>
      <h1 class="text-4xl sm:text-5xl font-bold text-slate-900 dark:text-slate-100 leading-tight">
        Eryandhi Putro<br />
        <span class="text-emerald-500 dark:text-cyan-400">Nugroho</span>
      </h1>
      <p class="mt-4 text-slate-500 dark:text-slate-400 max-w-xl font-mono text-sm leading-relaxed">
        // Software engineer from Ponorogo, Indonesia — building full-stack web systems,
        IoT solutions, and computer vision prototypes. Passionate about clean architecture,
        developer tooling, and shipping products that actually work.
      </p>

      <!-- Tags -->
      <div class="mt-5 flex flex-wrap gap-2">
        <span v-for="tag in ['Web Development', 'IoT', 'Computer Vision', 'ASP.NET Core', 'Vue 3']" :key="tag"
          class="px-2.5 py-1 text-xs font-mono rounded-md bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400 border border-slate-200 dark:border-slate-700"
        >{{ tag }}</span>
      </div>
    </section>

    <!-- Job History pipeline -->
    <section v-if="!loading">
      <h2 class="font-mono text-xs uppercase tracking-widest text-slate-400 dark:text-slate-500 mb-6 flex items-center gap-2">
        <span class="inline-block w-4 h-px bg-slate-300 dark:bg-slate-600" />
        experience.log
      </h2>
      <div class="relative space-y-0">
        <div
          v-for="(job, i) in jobs"
          :key="job.id"
          class="group relative pl-8 pb-10 last:pb-0"
        >
          <!-- Connector line -->
          <div v-if="i < jobs.length - 1" class="absolute left-[11px] top-5 bottom-0 w-px bg-slate-200 dark:bg-slate-700 group-hover:bg-emerald-400 dark:group-hover:bg-cyan-400 transition-colors" />
          <!-- Node -->
          <div class="absolute left-0 top-1 w-[22px] h-[22px] rounded border-2 border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 group-hover:border-emerald-400 dark:group-hover:border-cyan-400 transition-colors flex items-center justify-center">
            <div class="w-2 h-2 rounded-full bg-slate-300 dark:bg-slate-600 group-hover:bg-emerald-400 dark:group-hover:bg-cyan-400 transition-colors" />
          </div>

          <div class="border border-slate-200 dark:border-slate-700 rounded-lg p-4 group-hover:border-emerald-300 dark:group-hover:border-cyan-700 transition-all group-hover:shadow-sm">
            <div class="flex flex-wrap items-baseline gap-x-2 gap-y-1">
              <h3 class="font-semibold text-slate-900 dark:text-slate-100">{{ job.role }}</h3>
              <span class="text-emerald-600 dark:text-cyan-400 font-mono text-sm">@ {{ job.company }}</span>
              <span v-if="job.isCurrentRole" class="ml-auto text-xs font-mono bg-emerald-50 dark:bg-cyan-900/30 text-emerald-700 dark:text-cyan-400 border border-emerald-200 dark:border-cyan-800 rounded px-2 py-0.5">current</span>
            </div>
            <p class="text-xs font-mono text-slate-400 dark:text-slate-500 mt-1">
              {{ formatDate(job.startDate) }} → {{ job.isCurrentRole ? 'present' : (job.endDate ? formatDate(job.endDate) : '?') }}
            </p>
            <p v-if="job.description" class="mt-2 text-sm text-slate-600 dark:text-slate-400">{{ job.description }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Featured Projects -->
    <section v-if="!loading && projects.length">
      <h2 class="font-mono text-xs uppercase tracking-widest text-slate-400 dark:text-slate-500 mb-6 flex items-center gap-2">
        <span class="inline-block w-4 h-px bg-slate-300 dark:bg-slate-600" />
        projects/featured
      </h2>
      <div class="grid sm:grid-cols-2 gap-4">
        <div
          v-for="project in projects.slice(0, 4)"
          :key="project.id"
          class="group border border-slate-200 dark:border-slate-700 rounded-lg p-4 hover:border-emerald-300 dark:hover:border-cyan-700 transition-all hover:shadow-sm"
        >
          <h3 class="font-semibold text-slate-900 dark:text-slate-100 group-hover:text-emerald-600 dark:group-hover:text-cyan-400 transition-colors">{{ project.title }}</h3>
          <p class="mt-1 text-sm text-slate-500 dark:text-slate-400 line-clamp-2">{{ project.description }}</p>
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
    </section>

    <div v-if="loading" class="flex items-center gap-2 text-slate-400 font-mono text-sm">
      <span class="animate-pulse">▋</span> loading pipeline data...
    </div>

    <!-- Contact -->
    <section class="border border-slate-200 dark:border-slate-700 rounded-2xl overflow-hidden">
      <div class="bg-slate-50 dark:bg-slate-800/60 border-b border-slate-200 dark:border-slate-700 px-5 py-3 flex items-center gap-2">
        <span class="text-xs font-mono text-slate-400">stage</span>
        <span class="text-xs font-mono font-bold text-slate-700 dark:text-slate-300">contact</span>
        <span class="ml-auto w-2 h-2 rounded-full bg-emerald-500 dark:bg-cyan-400" />
      </div>
      <div class="p-8 bg-white dark:bg-slate-900">
        <div class="max-w-md space-y-6">
          <p class="text-slate-600 dark:text-slate-300 text-sm leading-relaxed">
            Have a project in mind or just want to say hi?<br />
            My inbox is always open.
          </p>

          <!-- Email -->
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-emerald-50 dark:bg-cyan-900/30 border border-emerald-200 dark:border-cyan-800 flex items-center justify-center shrink-0">
              <span class="text-xs text-emerald-600 dark:text-cyan-400">@</span>
            </div>
            <a
              href="mailto:hi@nayreisme.dev"
              class="font-mono text-sm text-emerald-600 dark:text-cyan-400 hover:underline"
            >hi@nayreisme.dev</a>
          </div>

          <!-- CV Download -->
          <a
            href="/api/cv/download"
            download
            class="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg border border-slate-200 dark:border-slate-700 text-xs font-mono text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800 hover:border-emerald-400 dark:hover:border-cyan-400 transition-colors"
          >
            <span class="text-emerald-500 dark:text-cyan-400">↓</span> download cv
          </a>
        </div>
      </div>
    </section>
  </div>
</template>
