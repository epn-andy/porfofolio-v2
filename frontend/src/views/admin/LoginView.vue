<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const router = useRouter()
const auth = useAuthStore()
const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleLogin() {
  error.value = ''
  loading.value = true
  try {
    await auth.login({ email: email.value, password: password.value })
    router.push('/admin/articles')
  } catch {
    error.value = 'Invalid credentials. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 dark:bg-slate-950 flex items-center justify-center p-4">
    <div class="w-full max-w-sm">
      <div class="mb-8 text-center">
        <p class="font-mono text-xs text-emerald-600 dark:text-cyan-400 uppercase tracking-widest mb-2">admin access</p>
        <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">Sign in</h1>
      </div>

      <form @submit.prevent="handleLogin" class="border border-slate-200 dark:border-slate-700 rounded-xl p-6 bg-white dark:bg-slate-900 space-y-4">
        <div>
          <label class="block text-xs font-mono text-slate-500 dark:text-slate-400 mb-1.5">email</label>
          <input
            v-model="email"
            type="email"
            required
            autocomplete="email"
            class="w-full border border-slate-200 dark:border-slate-700 rounded-lg px-3 py-2 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400 transition-colors font-mono"
          />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 dark:text-slate-400 mb-1.5">password</label>
          <input
            v-model="password"
            type="password"
            required
            autocomplete="current-password"
            class="w-full border border-slate-200 dark:border-slate-700 rounded-lg px-3 py-2 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400 transition-colors font-mono"
          />
        </div>

        <p v-if="error" class="text-xs text-red-500 font-mono">{{ error }}</p>

        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2 rounded-lg bg-emerald-500 dark:bg-cyan-500 text-white font-mono text-sm hover:bg-emerald-600 dark:hover:bg-cyan-600 transition-colors disabled:opacity-50"
        >
          {{ loading ? 'authenticating...' : '> login' }}
        </button>
      </form>
    </div>
  </div>
</template>
