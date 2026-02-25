import api from './api'

export interface Article {
  id: number
  title: string
  slug: string
  content: string
  excerpt: string | null
  published: boolean
  createdAt: string
  updatedAt: string
}

export type ArticleDto = Omit<Article, 'id' | 'createdAt' | 'updatedAt'>

export const articlesService = {
  getAll: () => api.get<Article[]>('/articles'),
  getBySlug: (slug: string) => api.get<Article>(`/articles/${slug}`),
  getAllAdmin: () => api.get<Article[]>('/articles/all'),
  create: (dto: ArticleDto) => api.post<Article>('/articles', dto),
  update: (id: number, dto: ArticleDto) => api.put<Article>(`/articles/${id}`, dto),
  delete: (id: number) => api.delete(`/articles/${id}`),
}
