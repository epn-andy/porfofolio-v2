<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { marked } from 'marked'
import DOMPurify from 'dompurify'
import { articlesService, type Article } from '../services/articles'

const route = useRoute()
const router = useRouter()
const article = ref<Article | null>(null)
const safeHtml = ref('')
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await articlesService.getBySlug(route.params.slug as string)
    article.value = res.data
    const parsed = await marked(res.data.content)
    safeHtml.value = DOMPurify.sanitize(parsed)
  } catch {
    router.push({ name: 'articles' })
  } finally {
    loading.value = false
  }
})

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
}
</script>

<template>
  <div class="pb-16 max-w-2xl">
    <div v-if="loading" class="text-slate-400 font-mono text-sm animate-pulse">▋ loading...</div>

    <article v-else-if="article">
      <div class="mb-6 flex items-center gap-3">
        <div class="w-2 h-2 rounded-full bg-emerald-500 dark:bg-cyan-400" />
        <span class="font-mono text-xs text-slate-400 dark:text-slate-500">{{ formatDate(article.createdAt) }}</span>
      </div>

      <h1 class="text-3xl font-bold text-slate-900 dark:text-slate-100 mb-6">{{ article.title }}</h1>

      <!-- Sanitized Markdown content -->
      <div
        class="prose prose-slate dark:prose-invert prose-sm max-w-none prose-code:font-mono prose-pre:bg-slate-100 dark:prose-pre:bg-slate-800 prose-a:text-emerald-600 dark:prose-a:text-cyan-400"
        v-html="safeHtml"
      />
    </article>
  </div>
</template>
