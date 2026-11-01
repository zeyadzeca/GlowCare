import axios, { AxiosError } from 'axios'
import type { ApiError } from '@/types/api.types'

/**
 * Central Axios instance for the whole app.
 *
 * - Base URL comes from VITE_API_URL (see .env). In development the Vite
 *   dev-server proxies /api -> the backend, so this stays same-origin.
 * - A request interceptor attaches the JWT from localStorage when present.
 * - A response interceptor normalises errors into a plain `ApiError` so every
 *   caller can read `.message` without unwrapping Axios itself.
 */

/** localStorage key holding the raw JWT issued by /api/auth/*. */
export const TOKEN_STORAGE_KEY = 'glowcare_token'

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_STORAGE_KEY)
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, token)
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY)
}

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? '/api',
  headers: { 'Content-Type': 'application/json' },
})

// ---------------------------------------------------------------------------
// Request: attach the JWT when we have one.
// ---------------------------------------------------------------------------
api.interceptors.request.use((config) => {
  const token = getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// ---------------------------------------------------------------------------
// Response: on 401 the token is missing/expired, so drop it and let the auth
// guard bounce the user to the login page. Errors are otherwise passed through
// unchanged - callers use getErrorMessage() from utils/errors.
// ---------------------------------------------------------------------------
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      clearToken()
    }
    return Promise.reject(error)
  },
)

/** Extracts the backend's { statusCode, message } shape when available. */
export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiError | undefined
    if (data?.message) {
      return { statusCode: data.statusCode ?? error.response?.status ?? 0, message: data.message }
    }
    return {
      statusCode: error.response?.status ?? 0,
      message: error.message,
    }
  }
  return { statusCode: 0, message: error instanceof Error ? error.message : 'Unexpected error.' }
}

export default api
