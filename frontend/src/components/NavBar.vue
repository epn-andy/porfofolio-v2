<script setup lang="ts">
import { RouterLink } from 'vue-router'
import ThemeToggle from './ThemeToggle.vue'
</script>

<template>
  <nav class="sticky top-0 z-50 border-b border-slate-200 dark:border-slate-700 bg-white/90 dark:bg-slate-900/90 backdrop-blur">
    <div class="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8 h-14 flex items-center justify-between">
      <!-- Logo / Name -->
      <RouterLink to="/" class="font-mono text-sm font-bold tracking-tight text-slate-900 dark:text-emerald-400">
        epn<span class="text-emerald-500 dark:text-cyan-400">://</span>portfolio
      </RouterLink>

      <!-- Nav links -->
      <div class="flex items-center gap-6">
        <RouterLink
          v-for="link in links"
          :key="link.to"
          :to="link.to"
          class="text-sm text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-emerald-400 transition-colors font-mono"
          active-class="text-slate-900 dark:text-emerald-400 font-semibold"
        >
          {{ link.label }}
        </RouterLink>
        <ThemeToggle />
      </div>
    </div>
  </nav>
</template>

<script lang="ts">
const links = [
  { to: '/', label: '~/' },
  { to: '/articles', label: './articles' },
  { to: '/projects', label: './projects' },
]
</script>
