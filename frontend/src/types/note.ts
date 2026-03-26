export interface Note {
  id: number
  title: string
  content: string
  createdAt: string
  updatedAt: string
  userId: number
}

export interface NotePayload {
  title: string
  content?: string
}

export type NoteFilterMode = 'all' | 'with_content' | 'empty_content'
export type NoteSortMode = 'newest' | 'oldest' | 'title_asc' | 'title_desc'
