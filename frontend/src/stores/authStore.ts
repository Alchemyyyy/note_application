import { defineStore } from 'pinia'
import { getApiErrorMessage, setAuthToken } from '../services/api'
import { loginUser, registerUser } from '../services/authService'
import type { AuthPayload } from '../types/auth'

const AUTH_TOKEN_KEY = 'notes_auth_token'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: '' as string,
    loading: false,
  }),
  getters: {
    isAuthenticated: (state) => Boolean(state.token),
  },
  actions: {
    hydrate() {
      const token = localStorage.getItem(AUTH_TOKEN_KEY)
      if (token && token.trim().length > 0) {
        this.token = token
        setAuthToken(token)
      }
    },
    async register(payload: AuthPayload) {
      this.loading = true
      try {
        const response = await registerUser(payload)
        const token = response.data?.data?.token?.trim() || ''
        if (!token) {
          throw new Error('Registration succeeded but no token was returned.')
        }
        this.setToken(token)
        return response.data?.message || 'Registered successfully.'
      } catch (error: unknown) {
        throw new Error(getApiErrorMessage(error, 'Failed to register.'))
      } finally {
        this.loading = false
      }
    },
    async login(payload: AuthPayload) {
      this.loading = true
      try {
        const response = await loginUser(payload)
        const token = response.data?.data?.token?.trim() || ''
        if (!token) {
          throw new Error('Login succeeded but no token was returned.')
        }
        this.setToken(token)
        return response.data?.message || 'Logged in successfully.'
      } catch (error: unknown) {
        throw new Error(getApiErrorMessage(error, 'Failed to login.'))
      } finally {
        this.loading = false
      }
    },
    logout() {
      this.token = ''
      localStorage.removeItem(AUTH_TOKEN_KEY)
      setAuthToken(null)
    },
    setToken(token: string) {
      this.token = token
      localStorage.setItem(AUTH_TOKEN_KEY, token)
      setAuthToken(token)
    },
  },
})
