import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/context/AuthContext'
import { useCart } from '@/context/CartContext'
import { useWishlist } from '@/context/WishlistContext'
import { getErrorMessage } from '@/utils/errors'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'

/**
 * Customer registration. The backend always registers a Customer; users apply
 * to become sellers later through the seller application flow.
 *
 * Password rules mirror the backend Identity configuration:
 * 8+ chars with upper, lower, digit and a non-alphanumeric character.
 */
export function RegisterPage() {
  const { register } = useAuth()
  const { mergeGuestCart } = useCart()
  const { mergeGuestWishlist } = useWishlist()
  const navigate = useNavigate()

  const [form, setForm] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
  })
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  function update(field: keyof typeof form, value: string) {
    setForm((previous) => ({ ...previous, [field]: value }))
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)

    if (form.password !== form.confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    setSubmitting(true)
    try {
      await register(form)
      // Merge guest cart after successful registration
      await mergeGuestCart()
      await mergeGuestWishlist()
      navigate('/', { replace: true })
    } catch (err) {
      setError(getErrorMessage(err, 'Unable to create your account.'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="shell py-12 lg:py-16">
      <div className="mx-auto grid max-w-5xl overflow-hidden rounded-3xl bg-white shadow-sm lg:grid-cols-2">
        {/* Brand panel - Primary Blue with yellow headline accent. */}
        <div className="flex flex-col justify-center bg-royal p-10 text-white lg:order-2">
          <p className="font-display text-2xl font-bold text-white">
            <span className="text-white">Glow</span><span className="text-yellow">Care</span>
          </p>
          <h2 className="mt-6 font-display font-bold uppercase">
            <span className="block text-display-sm text-white">Join the</span>
            <span className="block text-display-sm text-yellow">glow.</span>
          </h2>
          <p className="mt-4 max-w-xs text-white/85">
            Shop verified skincare, track every order and tell other customers
            what actually worked for your skin.
          </p>
          <div className="mt-8 h-1.5 w-24 rounded-full bg-yellow" />
        </div>

        {/* Form panel */}
        <div className="p-8 lg:p-10 lg:order-1">
          <h1 className="heading-panel">Create account</h1>
          <p className="mt-2 text-navy/60">Join GlowCare to shop, review and track orders.</p>

          {error && (
            <div className="mt-4 rounded-xl bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="mt-6">
          <Input
            label="Full name"
            name="fullName"
            autoComplete="name"
            required
            value={form.fullName}
            onChange={(e) => update('fullName', e.target.value)}
          />
          <Input
            label="Email"
            type="email"
            name="email"
            autoComplete="email"
            required
            value={form.email}
            onChange={(e) => update('email', e.target.value)}
          />
          <Input
            label="Password"
            type="password"
            name="password"
            autoComplete="new-password"
            required
            minLength={8}
            value={form.password}
            onChange={(e) => update('password', e.target.value)}
            error="8+ characters with upper, lower, a digit and a symbol."
          />
          <Input
            label="Confirm password"
            type="password"
            name="confirmPassword"
            autoComplete="new-password"
            required
            value={form.confirmPassword}
            onChange={(e) => update('confirmPassword', e.target.value)}
          />

          <Button type="submit" loading={submitting} className="mt-2 w-full">
            Create account
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-navy/60">
          Already have an account?{' '}
          <Link to="/login" className="font-semibold text-royal hover:underline">
            Sign in
          </Link>
        </p>
        </div>
      </div>
    </div>
  )
}
