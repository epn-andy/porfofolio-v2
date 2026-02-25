<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { marked } from 'marked'
import DOMPurify from 'dompurify'
import { articlesService, type Article, type ArticleDto } from '../../services/articles'
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()
const articles = ref<Article[]>([])
const loading = ref(true)
const saving = ref(false)
const showForm = ref(false)
const editing = ref<Article | null>(null)
const preview = ref('')

const form = ref<ArticleDto>({
  title: '', slug: '', content: '', excerpt: null, published: false,
})

async function loadArticles() {
  loading.value = true
  try {
    const res = await articlesService.getAllAdmin()
    articles.value = res.data
  } finally {
    loading.value = false
  }
}

onMounted(loadArticles)

function openCreate() {
  editing.value = null
  form.value = { title: '', slug: '', content: '', excerpt: null, published: false }
  preview.value = ''
  showForm.value = true
}

function openEdit(a: Article) {
  editing.value = a
  form.value = { title: a.title, slug: a.slug, content: a.content, excerpt: a.excerpt, published: a.published }
  updatePreview()
  showForm.value = true
}

async function updatePreview() {
  const parsed = await marked(form.value.content)
  preview.value = DOMPurify.sanitize(parsed)
}

function generateSlug() {
  form.value.slug = form.value.title
    .toLowerCase().trim()
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/\s+/g, '-')
}

async function saveArticle() {
  saving.value = true
  try {
    const dto: ArticleDto = {
      ...form.value,
      excerpt: form.value.excerpt?.trim() || null,
    }
    if (editing.value) {
      await articlesService.update(editing.value.id, dto)
      toast.success('Article updated.')
    } else {
      await articlesService.create(dto)
      toast.success('Article created.')
    }
    showForm.value = false
    await loadArticles()
  } finally {
    saving.value = false
  }
}

async function deleteArticle(id: number) {
  if (!confirm('Delete this article?')) return
  await articlesService.delete(id)
  toast.success('Article deleted.')
  await loadArticles()
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-mono text-sm font-bold text-slate-900 dark:text-slate-100">articles/</h1>
      <button @click="openCreate" class="px-3 py-1.5 rounded-lg bg-emerald-500 dark:bg-cyan-500 text-white text-xs font-mono hover:bg-emerald-600 dark:hover:bg-cyan-600 transition-colors">
        + new article
      </button>
    </div>

    <!-- Form / Editor -->
    <div v-if="showForm" class="mb-6 border border-slate-200 dark:border-slate-700 rounded-xl p-5 bg-white dark:bg-slate-900 space-y-4">
      <h2 class="font-mono text-xs text-slate-400">{{ editing ? 'edit article' : 'new article' }}</h2>
      <div class="grid sm:grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">title</label>
          <input v-model="form.title" @input="generateSlug" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 font-mono focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">slug</label>
          <input v-model="form.slug" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 font-mono focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
        </div>
      </div>
      <div>
        <label class="block text-xs font-mono text-slate-500 mb-1">excerpt</label>
        <input v-model="form.excerpt" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-1.5 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400" />
      </div>
      <div class="grid lg:grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">content (markdown)</label>
          <textarea v-model="form.content" @input="updatePreview" rows="16" class="w-full border border-slate-200 dark:border-slate-700 rounded px-3 py-2 text-sm bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 font-mono focus:outline-none focus:border-emerald-400 dark:focus:border-cyan-400 resize-none" />
        </div>
        <div>
          <label class="block text-xs font-mono text-slate-500 mb-1">preview</label>
          <div class="border border-slate-200 dark:border-slate-700 rounded p-3 min-h-[calc(16*1.5rem)] prose prose-sm prose-slate dark:prose-invert max-w-none overflow-auto" v-html="preview" />
        </div>
      </div>
      <div class="flex items-center gap-3">
        <label class="flex items-center gap-2 text-xs font-mono text-slate-600 dark:text-slate-400 cursor-pointer">
          <input v-model="form.published" type="checkbox" class="accent-emerald-500" />
          published
        </label>
        <div class="ml-auto flex gap-2">
          <button @click="showForm = false" class="px-3 py-1.5 text-xs font-mono border border-slate-200 dark:border-slate-700 rounded hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">cancel</button>
          <button @click="saveArticle" :disabled="saving" class="px-3 py-1.5 text-xs font-mono rounded bg-emerald-500 dark:bg-cyan-500 text-white hover:bg-emerald-600 dark:hover:bg-cyan-600 transition-colors disabled:opacity-50">{{ saving ? 'saving...' : 'save' }}</button>
        </div>
      </div>
    </div>

    <!-- List -->
    <div v-if="loading" class="text-slate-400 font-mono text-sm animate-pulse">▋ loading...</div>
    <div v-else class="space-y-2">
      <div v-for="a in articles" :key="a.id" class="flex items-center gap-3 border border-slate-200 dark:border-slate-700 rounded-lg px-4 py-3 bg-white dark:bg-slate-900">
        <span class="flex-1 font-medium text-sm text-slate-900 dark:text-slate-100 truncate">{{ a.title }}</span>
        <span class="text-xs font-mono" :class="a.published ? 'text-emerald-600 dark:text-cyan-400' : 'text-slate-400'">{{ a.published ? 'published' : 'draft' }}</span>
        <button @click="openEdit(a)" class="text-xs font-mono text-slate-500 hover:text-slate-900 dark:hover:text-slate-100 transition-colors">edit</button>
        <button @click="deleteArticle(a.id)" class="text-xs font-mono text-red-400 hover:text-red-600 transition-colors">delete</button>
      </div>
    </div>
  </div>
</template>
