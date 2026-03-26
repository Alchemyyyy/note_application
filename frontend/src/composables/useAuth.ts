import type { AuthPayload } from '../types/auth'

export function validateAuthPayload(payload: AuthPayload): string | null {
  if (!payload.username || !payload.password) {
    return 'Username and password are required.'
  }
  if (payload.username.length < 3 || payload.username.length > 50) {
    return 'Username must be between 3 and 50 characters.'
  }
  if (!/^[a-zA-Z0-9_]+$/.test(payload.username)) {
    return 'Username can only contain letters, numbers, and underscores.'
  }
  if (payload.password.length < 6 || payload.password.length > 128) {
    return 'Password must be between 6 and 128 characters.'
  }

  return null
}

export function isAuthError(message: string) {
  const lowered = message.toLowerCase()
  return lowered.includes('authentication') || lowered.includes('unauthorized') || lowered.includes('token')
}
