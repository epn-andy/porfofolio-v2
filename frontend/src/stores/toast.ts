import { ref } from 'vue'
import { defineStore } from 'pinia'

export type ToastType = 'error' | 'success' | 'warning' | 'info'

export interface Toast {
  id: number
  message: string
  type: ToastType
  duration: number
}

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<Toast[]>([])
  let nextId = 0

  function add(message: string, type: ToastType = 'error', duration = 4000) {
    const id = nextId++
    toasts.value.push({ id, message, type, duration })
    setTimeout(() => remove(id), duration)
  }

  function remove(id: number) {
    const idx = toasts.value.findIndex((t) => t.id === id)
    if (idx !== -1) toasts.value.splice(idx, 1)
  }

  const error = (msg: string) => add(msg, 'error')
  const success = (msg: string) => add(msg, 'success')
  const warning = (msg: string) => add(msg, 'warning')
  const info = (msg: string) => add(msg, 'info')

  return { toasts, add, remove, error, success, warning, info }
})
