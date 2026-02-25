<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { articlesService, type Article } from '../services/articles'

const articles = ref<Article[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await articlesService.getAll()
    articles.value = res.data
  } finally {
    loading.value = false
  }
})

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}
</script>

<template>
  <div class="pb-16">
    <div class="mb-8 flex items-center gap-3">
      <div class="w-2 h-2 rounded-full bg-emerald-500 dark:bg-cyan-400" />
      <h1 class="font-mono text-xs uppercase tracking-widest text-slate-400 dark:text-slate-500">articles/</h1>
    </div>

    <div v-if="loading" class="text-slate-400 font-mono text-sm animate-pulse">▋ loading...</div>

    <div v-else-if="!articles.length" class="text-slate-400 font-mono text-sm">// no articles published yet.</div>

    <div v-else class="space-y-4">
      <RouterLink
        v-for="article in articles"
        :key="article.id"
        :to="{ name: 'article-detail', params: { slug: article.slug } }"
        class="group block border border-slate-200 dark:border-slate-700 rounded-lg p-4 hover:border-emerald-300 dark:hover:border-cyan-700 transition-all hover:shadow-sm"
      >
        <h2 class="font-semibold text-slate-900 dark:text-slate-100 group-hover:text-emerald-600 dark:group-hover:text-cyan-400 transition-colors">{{ article.title }}</h2>
        <p v-if="article.excerpt" class="mt-1 text-sm text-slate-500 dark:text-slate-400">{{ article.excerpt }}</p>
        <p class="mt-2 text-xs font-mono text-slate-400 dark:text-slate-500">{{ formatDate(article.createdAt) }}</p>
      </RouterLink>
    </div>
  </div>
</template>
