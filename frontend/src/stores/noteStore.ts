import { defineStore } from 'pinia'
import { getApiErrorMessage } from '../services/api'
import { createNote, deleteNote, getNotes, updateNote } from '../services/noteService'
import type { Note, NotePayload } from '../types/note'

export const useNotesStore = defineStore('notes', {
  state: () => ({
    notes: [] as Note[],
    loading: false,
    saving: false,
    deletingIds: [] as number[],
  }),
  actions: {
    async fetchNotes() {
      this.loading = true
      try {
        const response = await getNotes()
        this.notes = response.data?.data || []
      } catch (error: unknown) {
        throw new Error(getApiErrorMessage(error, 'Failed to load notes.'))
      } finally {
        this.loading = false
      }
    },
    async createNote(payload: NotePayload) {
      this.saving = true
      try {
        const response = await createNote(payload)
        const created = response.data?.data
        if (created) {
          this.notes.unshift(created)
        }
        return created
      } catch (error: unknown) {
        throw new Error(getApiErrorMessage(error, 'Failed to create note.'))
      } finally {
        this.saving = false
      }
    },
    async updateNote(id: number, payload: NotePayload) {
      this.saving = true
      try {
        const response = await updateNote(id, payload)
        const updated = response.data?.data
        if (updated) {
          const index = this.notes.findIndex((note) => note.id === id)
          if (index !== -1) {
            this.notes[index] = updated
          }
        }
        return updated
      } catch (error: unknown) {
        throw new Error(getApiErrorMessage(error, 'Failed to update note.'))
      } finally {
        this.saving = false
      }
    },
    async deleteNote(id: number) {
      if (this.deletingIds.includes(id)) {
        return
      }
      this.deletingIds.push(id)
      try {
        await deleteNote(id)
        this.notes = this.notes.filter((note) => note.id !== id)
      } catch (error: unknown) {
        throw new Error(getApiErrorMessage(error, 'Failed to delete note.'))
      } finally {
        this.deletingIds = this.deletingIds.filter((value) => value !== id)
      }
    },
    clear() {
      this.notes = []
      this.saving = false
      this.deletingIds = []
    },
  },
})
