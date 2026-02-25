import api from './api'

export interface Project {
  id: number
  title: string
  description: string
  techStack: string | null
  liveUrl: string | null
  githubUrl: string | null
  imageUrl: string | null
  order: number
  createdAt: string
}

export type ProjectDto = Omit<Project, 'id' | 'createdAt'>

export const projectsService = {
  getAll: () => api.get<Project[]>('/projects'),
  getById: (id: number) => api.get<Project>(`/projects/${id}`),
  create: (dto: ProjectDto) => api.post<Project>('/projects', dto),
  update: (id: number, dto: ProjectDto) => api.put<Project>(`/projects/${id}`, dto),
  delete: (id: number) => api.delete(`/projects/${id}`),
}
