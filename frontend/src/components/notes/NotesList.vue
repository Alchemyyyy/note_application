<script setup lang="ts">
import BaseButton from '../base/BaseButton.vue'
import type { Note } from '../../types/note'
import { formatDate } from '../../utils/formatDate'

defineProps<{
  notes: Note[]
  selectedNoteId: number | null
  loading: boolean
  deletingIds: number[]
}>()

const emit = defineEmits<{
  (e: 'select', note: Note): void
  (e: 'edit', note: Note): void
  (e: 'delete', note: Note): void
}>()

</script>

<template>
  <div v-if="loading" class="rounded-xl border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
    Loading notes...
  </div>

  <transition-group v-else-if="notes.length" name="list" tag="ul" class="space-y-3">
    <li
      v-for="note in notes"
      :key="note.id"
      class="rounded-xl border p-4 transition duration-200"
      :class="selectedNoteId === note.id ? 'border-emerald-400 bg-emerald-50' : 'border-slate-200 bg-white hover:border-slate-300'"
    >
      <button class="w-full text-left" @click="emit('select', note)">
        <h3 class="font-semibold text-slate-900">{{ note.title }}</h3>
        <p class="mt-1 text-xs text-slate-500">Created: {{ formatDate(note.createdAt) }}</p>
        <p class="mt-2 line-clamp-2 text-sm text-slate-600">{{ note.content || 'No content' }}</p>
      </button>
      <div class="mt-3 flex gap-2">
        <BaseButton size="sm" variant="secondary" @click="emit('edit', note)">Edit</BaseButton>
        <BaseButton
          size="sm"
          variant="danger"
          :disabled="deletingIds.includes(note.id)"
          @click="emit('delete', note)"
        >
          {{ deletingIds.includes(note.id) ? 'Deleting...' : 'Delete' }}
        </BaseButton>
      </div>
    </li>
  </transition-group>

  <div v-else class="rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6 text-sm text-slate-600">
    No notes found for the current search/filter.
  </div>
</template>
