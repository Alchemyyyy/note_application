<script setup lang="ts">
import BaseButton from '../components/base/BaseButton.vue'

defineProps<{
  title: string
  subtitle: string
  showLogout?: boolean
}>()

const emit = defineEmits<{
  (e: 'logout'): void
}>()
</script>

<template>
  <main class="min-h-screen bg-gradient-to-b from-slate-100 via-white to-emerald-50 text-slate-900">
    <div class="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
      <header class="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p class="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">Notes Application</p>
          <h1 class="text-3xl font-black tracking-tight">{{ title }}</h1>
          <p class="mt-2 text-sm text-slate-600">{{ subtitle }}</p>
        </div>
        <BaseButton v-if="showLogout" variant="secondary" @click="emit('logout')">Logout</BaseButton>
      </header>

      <slot />
    </div>
  </main>
</template>
