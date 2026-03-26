<script setup lang="ts">
import { computed, reactive, watch } from 'vue'
import BaseButton from '../base/BaseButton.vue'
import type { Note } from '../../types/note'

const props = defineProps<{
  editingNote: Note | null
  disabled?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', payload: { title: string; content: string }): void
  (e: 'cancel'): void
}>()

const form = reactive({
  title: '',
  content: '',
})

const isEditing = computed(() => Boolean(props.editingNote))

watch(
  () => props.editingNote,
  (note) => {
    form.title = note?.title || ''
    form.content = note?.content || ''
  },
  { immediate: true },
)

function submit() {
  emit('submit', {
    title: form.title.trim(),
    content: form.content.trim(),
  })
}
</script>

<template>
  <section class="rounded-2xl border border-slate-200 bg-white p-5 shadow-lg ring-1 ring-slate-100">
    <h2 class="text-lg font-bold text-slate-900">{{ isEditing ? 'Edit note' : 'Create note' }}</h2>
    <form class="mt-4 space-y-3" @submit.prevent="submit">
      <div>
        <label class="mb-1 block text-sm font-medium text-slate-700">Title</label>
        <input
          v-model="form.title"
          class="w-full rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100"
          :disabled="disabled"
          maxlength="100"
          placeholder="Write a clear title"
          type="text"
        />
      </div>
      <div>
        <label class="mb-1 block text-sm font-medium text-slate-700">Content</label>
        <textarea
          v-model="form.content"
          class="min-h-32 w-full rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100"
          :disabled="disabled"
          maxlength="5000"
          placeholder="Optional content"
        />
      </div>

      <div class="flex gap-2">
        <BaseButton :disabled="disabled" :loading="disabled" type="submit" variant="primary">
          {{ isEditing ? 'Save changes' : 'Create note' }}
        </BaseButton>
        <BaseButton v-if="isEditing" :disabled="disabled" type="button" variant="secondary" @click="emit('cancel')">
          Cancel
        </BaseButton>
      </div>
    </form>
  </section>
</template>
