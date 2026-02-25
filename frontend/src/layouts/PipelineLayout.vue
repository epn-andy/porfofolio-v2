<script setup lang="ts">
import NavBar from '../components/NavBar.vue'
import { RouterView } from 'vue-router'
</script>

<template>
  <div class="min-h-screen bg-white dark:bg-slate-900 font-sans">
    <NavBar />

    <!-- Pipeline spine -->
    <div class="relative mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
      <!-- Left vertical pipeline line -->
      <div class="absolute left-8 top-0 bottom-0 w-px bg-slate-200 dark:bg-slate-700 hidden md:block" />

      <main class="md:pl-16 py-8">
        <RouterView />
      </main>
    </div>
  </div>
</template>
