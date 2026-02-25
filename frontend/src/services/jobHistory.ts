import api from './api'

export interface JobHistory {
  id: number
  company: string
  role: string
  description: string | null
  startDate: string
  endDate: string | null
  isCurrentRole: boolean
  order: number
}

export type JobHistoryDto = Omit<JobHistory, 'id'>

export const jobHistoryService = {
  getAll: () => api.get<JobHistory[]>('/jobhistory'),
  getById: (id: number) => api.get<JobHistory>(`/jobhistory/${id}`),
  create: (dto: JobHistoryDto) => api.post<JobHistory>('/jobhistory', dto),
  update: (id: number, dto: JobHistoryDto) => api.put<JobHistory>(`/jobhistory/${id}`, dto),
  delete: (id: number) => api.delete(`/jobhistory/${id}`),
}
