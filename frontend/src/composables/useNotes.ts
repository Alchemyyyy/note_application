import type { Note, NoteFilterMode, NoteSortMode } from '../types/note'

export function validateNotePayload(payload: { title: string; content: string }): string | null {
  if (!payload.title) {
    return 'Title is required.'
  }
  if (payload.title.length > 100) {
    return 'Title cannot exceed 100 characters.'
  }
  if (payload.content.length > 5000) {
    return 'Content cannot exceed 5000 characters.'
  }

  return null
}

export function filterAndSortNotes(
  notes: Note[],
  query: string,
  filterMode: NoteFilterMode,
  sortMode: NoteSortMode,
): Note[] {
  const loweredQuery = query.trim().toLowerCase()

  const filtered = notes.filter((note) => {
    const matchesQuery =
      loweredQuery.length === 0 ||
      note.title.toLowerCase().includes(loweredQuery) ||
      (note.content || '').toLowerCase().includes(loweredQuery)

    const hasContent = Boolean(note.content && note.content.trim().length > 0)
    const matchesFilter =
      filterMode === 'all' ||
      (filterMode === 'with_content' && hasContent) ||
      (filterMode === 'empty_content' && !hasContent)

    return matchesQuery && matchesFilter
  })

  filtered.sort((a, b) => {
    if (sortMode === 'newest') return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    if (sortMode === 'oldest') return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
    if (sortMode === 'title_asc') return a.title.localeCompare(b.title)
    return b.title.localeCompare(a.title)
  })

  return filtered
}
