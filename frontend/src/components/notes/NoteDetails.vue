<script setup lang="ts">
import type { Note } from '../../types/note'
import { formatDate } from '../../utils/formatDate'

defineProps<{
  note: Note | null
}>()

</script>

<template>
  <section class="rounded-2xl border border-slate-200 bg-white p-5 shadow-lg ring-1 ring-slate-100">
    <h2 class="text-lg font-bold text-slate-900">Selected note</h2>
    <div v-if="note" class="mt-3 space-y-2">
      <h3 class="text-base font-semibold text-slate-900">{{ note.title }}</h3>
      <p class="whitespace-pre-wrap text-sm text-slate-700">{{ note.content || 'No content' }}</p>
      <p class="text-xs text-slate-500">Created: {{ formatDate(note.createdAt) }}</p>
      <p class="text-xs text-slate-500">Updated: {{ formatDate(note.updatedAt) }}</p>
    </div>
    <p v-else class="mt-3 text-sm text-slate-600">Select a note from the list to view details.</p>
  </section>
</template>
