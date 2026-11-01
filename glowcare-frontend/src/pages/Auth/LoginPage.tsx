import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/context/AuthContext'
import { useCart } from '@/context/CartContext'
import { getErrorMessage } from '@/utils/errors'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'

/**
 * Login page. On success the AuthContext stores the JWT, and we send the user
 * to Home by default (or back to wherever they were heading before via `state.from`).
 */
export function LoginPage() {
  const { login } = useAuth()
  const { mergeGuestCart } = useCart()
  const navigate = useNavigate()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const response = await login({ email, password })
      // Merge guest cart after successful login
      await mergeGuestCart()
      // Route each role to its own area: sellers land on the seller dashboard,
      // admins on the admin panel, customers on the storefront.
      const destination = response.roles.includes('Admin')
        ? '/admin'
        : response.roles.includes('Seller')
          ? '/seller'
          : '/'
      console.info('[login-debug] ' + JSON.stringify({ roles: response.roles, destination, userRef: window.__traceUser = (window.__traceUser ?? 0) + 1 }))
      navigate(destination, { replace: true })
    } catch (err) {
      setError(getErrorMessage(err, 'Unable to sign in. Please check your credentials.'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="shell py-12 lg:py-16">
      <div className="mx-auto grid max-w-5xl overflow-hidden rounded-3xl bg-white shadow-sm lg:grid-cols-2">
        {/* Brand panel - Primary Yellow with oversized blue headline. */}
        <div className="flex flex-col justify-center bg-yellow p-10">
          <p className="font-display text-2xl font-bold">
            <span className="text-royal">Glow</span><span className="text-navy">Care</span>
          </p>
          <h2 className="mt-6 font-display font-bold uppercase text-royal">
            <span className="block text-display-sm">Welcome</span>
            <span className="block text-display-sm">back.</span>
          </h2>
          <p className="mt-4 max-w-xs text-royal/90">
            Sign in to track orders, save your wishlist and pick up where you
            left off.
          </p>
          <div className="mt-8 h-1.5 w-24 rounded-full bg-royal" />
        </div>

        {/* Form panel */}
        <div className="p-8 lg:p-10">
          <h1 className="heading-panel">Sign in</h1>
          <p className="mt-2 text-navy/60">Sign in to your GlowCare account.</p>

          {error && (
            <div className="mt-4 rounded-xl bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="mt-6">
          <Input
            label="Email"
            type="email"
            name="email"
            autoComplete="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <Input
            label="Password"
            type="password"
            name="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />

          <Button type="submit" loading={submitting} className="mt-2 w-full">
            Sign in
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-navy/60">
          New to GlowCare?{' '}
          <Link to="/register" className="font-semibold text-royal hover:underline">
            Create an account
          </Link>
        </p>
        </div>
      </div>
    </div>
  )
}
