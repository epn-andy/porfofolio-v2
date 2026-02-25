<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { cvService, type CvInfo } from '../../services/cv'
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()
const current = ref<CvInfo | null>(null)
const loading = ref(true)
const uploading = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)

async function loadCv() {
  loading.value = true
  try {
    const res = await cvService.getInfo()
    current.value = res.data
  } catch {
    current.value = null
  } finally {
    loading.value = false
  }
}

onMounted(loadCv)

function onFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  selectedFile.value = input.files?.[0] ?? null
}

async function upload() {
  if (!selectedFile.value) return
  uploading.value = true
  try {
    const res = await cvService.upload(selectedFile.value)
    current.value = res.data
    selectedFile.value = null
    if (fileInput.value) fileInput.value.value = ''
    toast.success('CV uploaded successfully.')
  } finally {
    uploading.value = false
  }
}

async function deleteCv() {
  if (!current.value) return
  if (!confirm('Delete the current CV?')) return
  await cvService.delete(current.value.id)
  current.value = null
  toast.success('CV deleted.')
}

function formatDate(d: string) {
  return new Date(d).toLocaleString('en-US', {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}
</script>

<template>
  <div class="max-w-xl space-y-6">
    <div class="flex items-center justify-between">
      <h1 class="font-mono text-sm font-bold text-slate-900 dark:text-slate-100">cv/</h1>
    </div>

    <!-- Current CV -->
    <div class="border border-slate-200 dark:border-slate-700 rounded-xl p-5 bg-white dark:bg-slate-900">
      <p class="text-xs font-mono text-slate-400 mb-3">current file</p>

      <div v-if="loading" class="text-slate-400 font-mono text-sm animate-pulse">▋ loading...</div>

      <div v-else-if="current" class="flex items-center gap-3">
        <!-- PDF icon -->
        <div class="w-10 h-10 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 flex items-center justify-center shrink-0">
          <span class="text-xs font-mono font-bold text-red-500">PDF</span>
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-slate-900 dark:text-slate-100 truncate">{{ current.fileName }}</p>
          <p class="text-xs font-mono text-slate-400">uploaded {{ formatDate(current.uploadedAt) }}</p>
        </div>
        <div class="flex gap-2 shrink-0">
          <a
            href="/api/cv/download"
            target="_blank"
            class="px-3 py-1.5 text-xs font-mono border border-slate-200 dark:border-slate-700 rounded hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors text-slate-600 dark:text-slate-300"
          >preview ↗</a>
          <button
            @click="deleteCv"
            class="px-3 py-1.5 text-xs font-mono text-red-400 hover:text-red-600 border border-red-200 dark:border-red-800 rounded hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
          >delete</button>
        </div>
      </div>

      <div v-else class="text-sm text-slate-400 font-mono py-2">
        no CV uploaded yet
      </div>
    </div>

    <!-- Upload new CV -->
    <div class="border border-slate-200 dark:border-slate-700 rounded-xl p-5 bg-white dark:bg-slate-900 space-y-4">
      <p class="text-xs font-mono text-slate-400">{{ current ? 'replace cv' : 'upload cv' }}</p>

      <div
        class="border-2 border-dashed border-slate-200 dark:border-slate-700 rounded-lg p-6 text-center hover:border-emerald-400 dark:hover:border-cyan-400 transition-colors cursor-pointer"
        @click="fileInput?.click()"
      >
        <p class="text-sm text-slate-500 dark:text-slate-400 font-mono">
          {{ selectedFile ? selectedFile.name : 'click to select PDF file' }}
        </p>
        <p class="text-xs text-slate-400 font-mono mt-1">PDF only · max 10 MB</p>
        <input
          ref="fileInput"
          type="file"
          accept=".pdf,application/pdf"
          class="hidden"
          @change="onFileChange"
        />
      </div>

      <div class="flex justify-end">
        <button
          @click="upload"
          :disabled="!selectedFile || uploading"
          class="px-4 py-2 text-xs font-mono rounded-lg bg-emerald-500 dark:bg-cyan-500 text-white hover:bg-emerald-600 dark:hover:bg-cyan-600 transition-colors disabled:opacity-40"
        >
          {{ uploading ? 'uploading...' : '↑ upload' }}
        </button>
      </div>
    </div>

    <p class="text-xs font-mono text-slate-400">
      The CV is publicly downloadable at
      <code class="text-emerald-600 dark:text-cyan-400">/api/cv/download</code>
    </p>
  </div>
</template>
