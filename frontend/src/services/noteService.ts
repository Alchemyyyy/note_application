import { api } from './api'
import type { ApiResponse } from '../types/api'
import type { Note, NotePayload } from '../types/note'

export async function getNotes() {
  return api.get<ApiResponse<Note[]>>('/api/notes')
}

export async function createNote(payload: NotePayload) {
  return api.post<ApiResponse<Note>>('/api/notes', payload)
}

export async function updateNote(id: number, payload: NotePayload) {
  return api.put<ApiResponse<Note>>(`/api/notes/${id}`, payload)
}

export async function deleteNote(id: number) {
  return api.delete(`/api/notes/${id}`)
}
