import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/context/AuthContext'
import type { Role } from '@/types/enums'
import { Spinner } from '@/components/common/States'

/**
 * Route guards.
 *
 * These are a UX convenience, NOT the security boundary - the API enforces the
 * same rules independently (401 without a token, 403 for the wrong role).
 */

interface GuardProps {
  /** When given, the user must hold at least one of these roles. */
  roles?: Role[]
  children?: React.ReactNode
}

function useGuardLogic(roles?: Role[]) {
  const { user, isAuthenticated, loading } = useAuth()
  const location = useLocation()

  const hasRequiredRole =
    roles === undefined || (user?.roles.some((role) => roles.includes(role)) ?? false)

  if (roles) console.debug('[guard-debug]', { roles, userRoles: user?.roles ?? null, isAuthenticated, loading, allowed: isAuthenticated && hasRequiredRole })
  return { loading, allowed: isAuthenticated && hasRequiredRole, location }
}

/** Requires a signed-in user; otherwise redirects to /login. */
export function ProtectedRoute({ roles, children }: GuardProps) {
  const { loading, allowed, location } = useGuardLogic(roles)

  if (loading) return <Spinner label="Checking your session..." />

  if (!allowed) {
    // `state.from` lets the login page send the user back where they aimed.
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return <>{children ?? <Outlet />}</>
}

/** Requires one of the given roles; bounces unauthorized users home. */
export function RoleRoute({ roles, children }: GuardProps) {
  const { loading, allowed, location } = useGuardLogic(roles)

  if (loading) return <Spinner label="Checking your session..." />

  if (!allowed) {
    return <Navigate to="/" replace state={{ from: location }} />
  }

  return <>{children ?? <Outlet />}</>
}

/** Signed-in users should not see login/register again. */
export function GuestOnlyRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, loading } = useAuth()

  if (loading) return <Spinner />

  if (isAuthenticated) return <Navigate to="/" replace />

  return <>{children}</>
}
