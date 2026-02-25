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
        { path: 'cv', name: 'admin-cv', component: () => import('../views/admin/CvEditorView.vue') },
      ],
    },
  ],
})

router.beforeEach(async (to) => {
  const requiresAuth = to.matched.some((r) => r.meta.requiresAuth)
  if (requiresAuth) {
    const auth = useAuthStore()
    await auth.fetchMe()
    if (!auth.isAuthenticated) {
      return { name: 'admin-login' }
    }
  }

  // Redirect already-authenticated users away from the login page
  if (to.name === 'admin-login') {
    const auth = useAuthStore()
    await auth.fetchMe()
    if (auth.isAuthenticated) {
      return { name: 'admin-articles' }
    }
  }
})

export default router
