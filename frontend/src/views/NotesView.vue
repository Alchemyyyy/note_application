<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import NoteDetails from '../components/notes/NoteDetails.vue'
import NoteEditor from '../components/notes/NoteEditor.vue'
import NotesList from '../components/notes/NotesList.vue'
import NotesToolbar from '../components/notes/NotesToolbar.vue'
import { isAuthError } from '../composables/useAuth'
import { filterAndSortNotes, validateNotePayload } from '../composables/useNotes'
import MainLayout from '../layouts/MainLayout.vue'
import { useAuthStore } from '../stores/authStore'
import { useNotesStore } from '../stores/noteStore'
import type { Note, NoteFilterMode, NoteSortMode } from '../types/note'

const router = useRouter()
const authStore = useAuthStore()
const notesStore = useNotesStore()

const editingId = ref<number | null>(null)
const selectedNoteId = ref<number | null>(null)
const searchQuery = ref('')
const filterMode = ref<NoteFilterMode>('all')
const sortMode = ref<NoteSortMode>('newest')

const statusMessage = ref('')
const errorMessage = ref('')
const isWorking = computed(() => notesStore.loading || notesStore.saving)

const filteredNotes = computed(() =>
  filterAndSortNotes(notesStore.notes, searchQuery.value, filterMode.value, sortMode.value),
)

const selectedNote = computed(() => {
  if (!selectedNoteId.value) return null
  return notesStore.notes.find((note) => note.id === selectedNoteId.value) || null
})

const editingNote = computed(() => {
  if (!editingId.value) return null
  return notesStore.notes.find((note) => note.id === editingId.value) || null
})

const notesSummary = computed(() => {
  const total = notesStore.notes.length
  const withContent = notesStore.notes.filter((n) => (n.content || '').trim().length > 0).length
  const emptyContent = total - withContent
  return { total, withContent, emptyContent }
})

onMounted(async () => {
  await loadNotes()
})

async function loadNotes() {
  clearAlerts()
  try {
    await notesStore.fetchNotes()
    if (!selectedNoteId.value && filteredNotes.value.length > 0) {
      selectedNoteId.value = filteredNotes.value[0].id
    }
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to load notes.'
    if (isAuthError(message)) {
      logout()
      errorMessage.value = 'Session expired. Please login again.'
      return
    }
    errorMessage.value = message
  }
}

function logout() {
  authStore.logout()
  notesStore.clear()
  selectedNoteId.value = null
  editingId.value = null
  router.push('/login')
}

async function submitNote(payload: { title: string; content: string }) {
  clearAlerts()

  const validationMessage = validateNotePayload(payload)
  if (validationMessage) {
    errorMessage.value = validationMessage
    return
  }

  try {
    if (editingId.value) {
      const updated = await notesStore.updateNote(editingId.value, payload)
      if (updated) {
        selectedNoteId.value = updated.id
      }
      statusMessage.value = 'Note updated successfully.'
    } else {
      const created = await notesStore.createNote(payload)
      if (created) {
        selectedNoteId.value = created.id
      }
      statusMessage.value = 'Note created successfully.'
    }

    editingId.value = null
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to save note.'
    if (isAuthError(message)) {
      logout()
      errorMessage.value = 'Session expired. Please login again.'
      return
    }
    errorMessage.value = message
  }
}

function startEdit(note: Note) {
  clearAlerts()
  editingId.value = note.id
  selectedNoteId.value = note.id
}

function cancelEdit() {
  editingId.value = null
}

async function removeNote(note: Note) {
  const confirmed = window.confirm(`Delete \"${note.title}\"?`)
  if (!confirmed) return

  clearAlerts()

  try {
    await notesStore.deleteNote(note.id)
    if (selectedNoteId.value === note.id) {
      selectedNoteId.value = filteredNotes.value[0]?.id || null
    }
    if (editingId.value === note.id) {
      cancelEdit()
    }
    statusMessage.value = 'Note deleted successfully.'
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to delete note.'
    if (isAuthError(message)) {
      logout()
      errorMessage.value = 'Session expired. Please login again.'
      return
    }
    errorMessage.value = message
  }
}

function clearAlerts() {
  statusMessage.value = ''
  errorMessage.value = ''
}

function selectNote(note: Note) {
  selectedNoteId.value = note.id
}
</script>

<template>
  <MainLayout
    show-logout
    subtitle="Create, search, filter, sort, update, and delete your notes."
    title="Personal Notes Workspace"
    @logout="logout"
  >
    <div class="mb-4 grid gap-2 sm:grid-cols-3">
      <div class="rounded-xl border border-slate-200 bg-white/80 px-4 py-3">
        <p class="text-xs uppercase tracking-wide text-slate-500">Total</p>
        <p class="text-xl font-black text-slate-900">{{ notesSummary.total }}</p>
      </div>
      <div class="rounded-xl border border-slate-200 bg-white/80 px-4 py-3">
        <p class="text-xs uppercase tracking-wide text-slate-500">With Content</p>
        <p class="text-xl font-black text-emerald-700">{{ notesSummary.withContent }}</p>
      </div>
      <div class="rounded-xl border border-slate-200 bg-white/80 px-4 py-3">
        <p class="text-xs uppercase tracking-wide text-slate-500">Empty Content</p>
        <p class="text-xl font-black text-amber-700">{{ notesSummary.emptyContent }}</p>
      </div>
    </div>

    <transition name="slide-fade">
      <div
        v-if="statusMessage"
        class="mb-4 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
      >
        {{ statusMessage }}
      </div>
    </transition>
    <transition name="slide-fade">
      <div
        v-if="errorMessage"
        class="mb-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"
      >
        {{ errorMessage }}
      </div>
    </transition>

    <section class="grid gap-6 lg:grid-cols-[1.2fr_1fr]">
      <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-lg ring-1 ring-slate-100">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-lg font-black text-slate-900">Your Notes</h2>
          <p class="rounded-lg bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-700">
            {{ filteredNotes.length }} shown
          </p>
        </div>

        <NotesToolbar
          :disabled="isWorking"
          :filter-mode="filterMode"
          :search-query="searchQuery"
          :sort-mode="sortMode"
          @refresh="loadNotes"
          @update:filter-mode="filterMode = $event"
          @update:search-query="searchQuery = $event"
          @update:sort-mode="sortMode = $event"
        />

        <NotesList
          :deleting-ids="notesStore.deletingIds"
          :loading="notesStore.loading"
          :notes="filteredNotes"
          :selected-note-id="selectedNoteId"
          @delete="removeNote"
          @edit="startEdit"
          @select="selectNote"
        />
      </div>

      <div class="space-y-6">
        <NoteEditor :disabled="notesStore.saving" :editing-note="editingNote" @cancel="cancelEdit" @submit="submitNote" />
        <NoteDetails :note="selectedNote" />
      </div>
    </section>
  </MainLayout>
</template>
