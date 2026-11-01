import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import type { AuthResponse, LoginRequest, RegisterRequest, UserProfile } from '@/types/api.types'
import type { Role } from '@/types/enums'
import { authApi, clearToken, getToken, setToken, userApi } from '@/services/api'

/**
 * AuthContext holds the signed-in user and exposes login/register/logout.
 *
 * On mount we read the persisted JWT. If one exists we call
 * GET /api/users/me to resolve the current profile (and roles). If that call
 * fails with 401 the token is stale, so we simply treat the user as signed out.
 *
 * Roles come from the backend, never from anything the client invents - the
 * role guards below only ever read `user.roles`.
 */
interface AuthContextValue {
  /** True while the initial /users/me check is running. */
  loading: boolean
  user: UserProfile | null
  isAuthenticated: boolean
  login: (payload: LoginRequest) => Promise<AuthResponse>
  register: (payload: RegisterRequest) => Promise<AuthResponse>
  logout: () => void
  /** Refreshes the profile from the API (e.g. after updating it). */
  refreshProfile: () => Promise<void>
  hasRole: (role: Role) => boolean
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null)
  const [loading, setLoading] = useState<boolean>(getToken() !== null)

  // Resolve the profile for any persisted token when the app boots.
  useEffect(() => {
    let cancelled = false

    async function bootstrap() {
      if (!getToken()) {
        setLoading(false)
        return
      }
      try {
        const profile = await userApi.getProfile()
        if (!cancelled) setUser(profile)
      } catch {
        // 401 was already cleared by the Axios interceptor; anything else we
        // also treat as signed-out so the UI never shows a half-authenticated state.
        if (!cancelled) {
          clearToken()
          setUser(null)
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void bootstrap()
    return () => {
      cancelled = true
    }
  }, [])

  const login = useCallback(async (payload: LoginRequest) => {
    const response = await authApi.login(payload)
    setToken(response.token)
    // Rebuild the profile shape straight from the login response so the UI
    // updates instantly without an extra round-trip.
    setUser({
      id: response.userId,
      fullName: response.fullName,
      email: response.email,
      phoneNumber: null,
      roles: response.roles,
    })
    return response
  }, [])

  const register = useCallback(async (payload: RegisterRequest) => {
    const response = await authApi.register(payload)
    setToken(response.token)
    setUser({
      id: response.userId,
      fullName: response.fullName,
      email: response.email,
      phoneNumber: null,
      roles: response.roles,
    })
    return response
  }, [])

  const logout = useCallback(() => {
    clearToken()
    setUser(null)
  }, [])

  const refreshProfile = useCallback(async () => {
    if (!getToken()) return
    try {
      setUser(await userApi.getProfile())
    } catch {
      // Leave the current state alone on a transient failure.
    }
  }, [])

  const hasRole = useCallback(
    (role: Role) => user?.roles.includes(role) ?? false,
    [user],
  )

  const value = useMemo<AuthContextValue>(
    () => ({
      loading,
      user,
      isAuthenticated: user !== null,
      login,
      register,
      logout,
      refreshProfile,
      hasRole,
    }),
    [loading, user, login, register, logout, refreshProfile, hasRole],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
