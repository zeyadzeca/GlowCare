import { isAxiosError } from 'axios'
import type { ApiError } from '@/types/api.types'

/**
 * The backend returns every error as `{ statusCode, message }` from the global
 * ExceptionHandlingMiddleware. Validation failures (400 from [ApiController])
 * instead return ASP.NET's standard ValidationProblemDetails shape, which uses
 * a `errors` map of field -> messages.
 *
 * `getErrorMessage` normalises both shapes so pages can always show a single
 * human-readable message without caring where it came from.
 */
export function getErrorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  if (isAxiosError(error)) {
    const data = error.response?.data as ApiError | undefined

    // Business error thrown by a service (AppException -> middleware).
    if (data && typeof data.message === 'string' && data.message.length > 0) {
      return data.message
    }

    // ASP.NET model validation: { errors: { Field: ["msg", ...] } }
    const problem = error.response?.data as
      | { errors?: Record<string, string[]> }
      | undefined
    if (problem?.errors) {
      const messages = Object.values(problem.errors).flat()
      if (messages.length > 0) return messages.join(' ')
    }

    if (error.response) {
      switch (error.response.status) {
        case 401:
          return 'You need to sign in to continue.'
        case 403:
          return 'You do not have permission to do that.'
        case 404:
          return 'The requested item was not found.'
        case 409:
          return 'That action conflicts with the current state.'
        default:
          return `Request failed (${error.response.status}).`
      }
    }

    // No response at all - backend unreachable.
    return 'Cannot reach the server. Please make sure the API is running.'
  }

  if (error instanceof Error && error.message) return error.message

  return fallback
}

/** HTTP status of an Axios error, or 0 when the server was unreachable. */
export function getErrorStatus(error: unknown): number {
  if (isAxiosError(error)) return error.response?.status ?? 0
  return 0
}
