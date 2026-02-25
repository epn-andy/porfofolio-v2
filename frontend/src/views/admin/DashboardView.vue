<script setup lang="ts">
import { RouterView, RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import ThemeToggle from '../../components/ThemeToggle.vue'

const auth = useAuthStore()
const router = useRouter()

async function handleLogout() {
  await auth.logout()
  router.push('/admin/login')
}

const navItems = [
  { to: '/admin/articles', label: 'articles' },
  { to: '/admin/projects', label: 'projects' },
  { to: '/admin/jobs', label: 'job history' },
  { to: '/admin/cv', label: 'cv' },
]
</script>

<template>
  <div class="min-h-screen bg-slate-50 dark:bg-slate-950 flex">
    <!-- Sidebar -->
    <aside class="w-52 border-r border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 flex flex-col">
      <div class="h-14 flex items-center px-4 border-b border-slate-200 dark:border-slate-700">
        <span class="font-mono text-xs font-bold text-emerald-600 dark:text-cyan-400">admin://cms</span>
      </div>
      <nav class="flex-1 p-3 space-y-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-mono text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 hover:text-slate-900 dark:hover:text-emerald-400 transition-colors"
          active-class="bg-emerald-50 dark:bg-cyan-900/20 text-emerald-700 dark:text-cyan-400"
        >
          <span>./{{ item.label }}</span>
        </RouterLink>
      </nav>
      <div class="p-3 border-t border-slate-200 dark:border-slate-700 space-y-2">
        <p class="text-xs font-mono text-slate-400 px-3 truncate">{{ auth.email }}</p>
        <button
          @click="handleLogout"
          class="w-full text-left px-3 py-2 rounded-lg text-xs font-mono text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
        >logout</button>
      </div>
    </aside>

    <!-- Main -->
    <div class="flex-1 flex flex-col overflow-hidden">
      <header class="h-14 border-b border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 flex items-center justify-between px-6">
        <span class="font-mono text-xs text-slate-400">pipeline://admin-cms</span>
        <ThemeToggle />
      </header>
      <main class="flex-1 overflow-auto p-6">
        <RouterView />
      </main>
    </div>
  </div>
</template>
