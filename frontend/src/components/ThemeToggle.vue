<script setup lang="ts">
import { useThemeStore } from '../stores/theme'

const theme = useThemeStore()
</script>

<template>
  <button
    @click="theme.toggle()"
    class="p-1.5 rounded border border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-400 hover:border-emerald-400 dark:hover:border-cyan-400 hover:text-emerald-500 dark:hover:text-cyan-400 transition-colors"
    :aria-label="theme.isDark ? 'Switch to light mode' : 'Switch to dark mode'"
  >
    <span v-if="theme.isDark" class="font-mono text-xs">☀</span>
    <span v-else class="font-mono text-xs">☾</span>
  </button>
</template>
