import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../services/api'

export const useAuthStore = defineStore('auth', () => {
  const email = ref<string | null>(null)
  const checked = ref(false)

  const isAuthenticated = computed(() => email.value !== null)

  async function fetchMe() {
    if (checked.value) return
    try {
      const res = await api.get<{ email: string }>('/auth/me')
      email.value = res.data.email
    } catch {
      email.value = null
    } finally {
      checked.value = true
    }
  }

  async function login(credentials: { email: string; password: string }) {
    await api.post('/auth/login', credentials)
    checked.value = false
    await fetchMe()
  }

  async function logout() {
    await api.post('/auth/logout')
    email.value = null
    checked.value = false
  }

  return { email, isAuthenticated, fetchMe, login, logout }
})
