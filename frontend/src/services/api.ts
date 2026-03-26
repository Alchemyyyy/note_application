import axios from 'axios'
import type { AxiosError } from 'axios'
import type { ApiResponse } from '../types/api'

const rawApiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5005'
const apiBaseUrl = rawApiBaseUrl.replace(/\/+$/, '')

export const api = axios.create({
  baseURL: apiBaseUrl,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

export function setAuthToken(token: string | null) {
  if (token) {
    api.defaults.headers.common.Authorization = `Bearer ${token}`
  } else {
    delete api.defaults.headers.common.Authorization
  }
}

export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ApiResponse<unknown>>
    const apiMessage = axiosError.response?.data?.message
    if (apiMessage && apiMessage.trim().length > 0) {
      return apiMessage
    }
    if (axiosError.code === 'ECONNABORTED') {
      return 'Request timed out. Please try again.'
    }
  }
  return fallback
}
