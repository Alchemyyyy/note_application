import { api } from './api'
import type { ApiResponse } from '../types/api'
import type { AuthPayload, AuthTokenPayload } from '../types/auth'

export async function registerUser(payload: AuthPayload) {
  return api.post<ApiResponse<AuthTokenPayload>>('/api/auth/register', payload)
}

export async function loginUser(payload: AuthPayload) {
  return api.post<ApiResponse<AuthTokenPayload>>('/api/auth/login', payload)
}
