import api from './api'

export interface CvInfo {
  id: number
  fileName: string
  contentType: string
  uploadedAt: string
}

export const cvService = {
  getInfo: () => api.get<CvInfo>('/cv'),
  upload: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return api.post<CvInfo>('/cv', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
  delete: (id: number) => api.delete(`/cv/${id}`),
}
