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
    const toast = useToastStore()
    const data = error.response?.data

    // Use structured problem detail message if available, else fallback
    const message: string =
      data?.title ??
      data?.error ??
      error.message ??
      'An unexpected error occurred.'

    toast.error(message)
    return Promise.reject(error)
  },
)

export default api
