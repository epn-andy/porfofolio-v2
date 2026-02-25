import axios from 'axios'
import { useToastStore } from '@/stores/toast'

const api = axios.create({
  baseURL: '/api',
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status

    // 401 from /auth/me is expected (not-logged-in check) — don't toast
    const url: string = error.config?.url ?? ''
    if (status === 401 && url.includes('/auth/me')) {
      return Promise.reject(error)
    }

    const data = error.response?.data
    const message: string =
      data?.title ??
      data?.error ??
      error.message ??
      'An unexpected error occurred.'

    const toast = useToastStore()
    toast.error(message)
    return Promise.reject(error)
  },
)

export default api
