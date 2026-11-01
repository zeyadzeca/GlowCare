import api from './client'
import type { AuthResponse, LoginRequest, RegisterRequest } from '@/types/api.types'

/** POST /api/auth/register - registers a Customer and returns a JWT immediately. */
export async function register(payload: RegisterRequest): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/auth/register', payload)
  return data
}

/** POST /api/auth/login - email/password login, returns a JWT. */
export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/auth/login', payload)
  return data
}
