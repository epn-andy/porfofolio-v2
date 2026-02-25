import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: () => import('../layouts/PipelineLayout.vue'),
      children: [
        { path: '', name: 'home', component: () => import('../views/HomeView.vue') },
        { path: 'articles', name: 'articles', component: () => import('../views/ArticlesView.vue') },
        { path: 'articles/:slug', name: 'article-detail', component: () => import('../views/ArticleDetailView.vue') },
        { path: 'projects', name: 'projects', component: () => import('../views/ProjectsView.vue') },
      ],
    },
    {
      path: '/admin/login',
      name: 'admin-login',
      component: () => import('../views/admin/LoginView.vue'),
    },
    {
      path: '/admin',
      component: () => import('../views/admin/DashboardView.vue'),
      meta: { requiresAuth: true },
      children: [
        { path: '', redirect: '/admin/articles' },
        { path: 'articles', name: 'admin-articles', component: () => import('../views/admin/ArticleEditorView.vue') },
        { path: 'projects', name: 'admin-projects', component: () => import('../views/admin/ProjectEditorView.vue') },
        { path: 'jobs', name: 'admin-jobs', component: () => import('../views/admin/JobHistoryEditorView.vue') },
      ],
    },
  ],
})

router.beforeEach(async (to) => {
  if (to.meta.requiresAuth) {
    const auth = useAuthStore()
    await auth.fetchMe()
    if (!auth.isAuthenticated) {
      return { name: 'admin-login' }
    }
  }
})

export default router
