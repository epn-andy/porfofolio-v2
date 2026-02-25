<script setup lang="ts">
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()

const icons: Record<string, string> = {
  error: '✕',
  success: '✓',
  warning: '⚠',
  info: 'ℹ',
}

const colors: Record<string, string> = {
  error: 'bg-red-900/90 border-red-500 text-red-100',
  success: 'bg-emerald-900/90 border-emerald-500 text-emerald-100',
  warning: 'bg-yellow-900/90 border-yellow-500 text-yellow-100',
  info: 'bg-cyan-900/90 border-cyan-500 text-cyan-100',
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed bottom-6 right-6 z-50 flex flex-col gap-3 max-w-sm w-full pointer-events-none">
      <TransitionGroup
        name="toast"
        tag="div"
        class="flex flex-col gap-3"
      >
        <div
          v-for="t in toast.toasts"
          :key="t.id"
          :class="[
            'flex items-start gap-3 px-4 py-3 rounded border font-mono text-sm shadow-lg pointer-events-auto',
            colors[t.type],
          ]"
        >
          <span class="text-base leading-tight mt-px shrink-0">{{ icons[t.type] }}</span>
          <span class="flex-1 leading-snug">{{ t.message }}</span>
          <button
            class="shrink-0 opacity-60 hover:opacity-100 transition-opacity ml-2"
            @click="toast.remove(t.id)"
          >✕</button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.25s ease;
}
.toast-enter-from {
  opacity: 0;
  transform: translateX(1rem);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(1rem);
}
</style>
