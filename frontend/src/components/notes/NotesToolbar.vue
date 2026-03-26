<script setup lang="ts">
import BaseButton from '../base/BaseButton.vue'

defineProps<{
  searchQuery: string
  filterMode: 'all' | 'with_content' | 'empty_content'
  sortMode: 'newest' | 'oldest' | 'title_asc' | 'title_desc'
  disabled?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:searchQuery', value: string): void
  (e: 'update:filterMode', value: 'all' | 'with_content' | 'empty_content'): void
  (e: 'update:sortMode', value: 'newest' | 'oldest' | 'title_asc' | 'title_desc'): void
  (e: 'refresh'): void
}>()
</script>

<template>
  <div class="mb-4 grid gap-3 sm:grid-cols-3">
    <input
      :value="searchQuery"
      :disabled="disabled"
      class="rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 sm:col-span-3"
      placeholder="Search by title or content"
      type="text"
      @input="emit('update:searchQuery', ($event.target as HTMLInputElement).value)"
    />
    <select
      :value="filterMode"
      :disabled="disabled"
      class="rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100"
      @change="emit('update:filterMode', ($event.target as HTMLSelectElement).value as 'all' | 'with_content' | 'empty_content')"
    >
      <option value="all">All notes</option>
      <option value="with_content">With content</option>
      <option value="empty_content">Empty content</option>
    </select>
    <select
      :value="sortMode"
      :disabled="disabled"
      class="rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100"
      @change="emit('update:sortMode', ($event.target as HTMLSelectElement).value as 'newest' | 'oldest' | 'title_asc' | 'title_desc')"
    >
      <option value="newest">Newest</option>
      <option value="oldest">Oldest</option>
      <option value="title_asc">Title A-Z</option>
      <option value="title_desc">Title Z-A</option>
    </select>
    <BaseButton :disabled="disabled" variant="secondary" @click="emit('refresh')">Refresh</BaseButton>
  </div>
</template>
