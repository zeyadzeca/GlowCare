import { useEffect, useState, type FormEvent } from 'react'
import { Trash2 } from 'lucide-react'
import type { Review } from '@/types/api.types'
import { reviewWishlistApi } from '@/services/api'
import { useAuth } from '@/context/AuthContext'
import { getErrorMessage } from '@/utils/errors'
import { formatDateTime } from '@/utils/format'
import { StarRating } from '@/components/common/StarRating'
import { Button } from '@/components/common/Button'
import { EmptyState, Spinner } from '@/components/common/States'

/**
 * Reviews block for one product.
 *
 * - Reading is anonymous (GET /api/products/{id}/reviews).
 * - Writing requires the Customer role AND a verified, delivered purchase -
 *   the backend enforces both (403 / 400 / 409). We surface its exact message
 *   instead of trying to predict the rule client-side.
 * - When the signed-in user already wrote a review, we switch that card into
 *   edit / delete mode (PUT/DELETE /api/reviews/{id}).
 */
export function ReviewsSection({
  productId,
  currentUserId,
  onReviewsChanged,
}: {
  productId: number
  currentUserId?: string
  onReviewsChanged?: () => void
}) {
  const { isAuthenticated, hasRole } = useAuth()
  const isCustomer = hasRole('Customer')

  const [reviews, setReviews] = useState<Review[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  // Create form
  const [rating, setRating] = useState(5)
  const [comment, setComment] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [formNotice, setFormNotice] = useState<string | null>(null)

  // Edit state for the user's own review
  const [editingId, setEditingId] = useState<number | null>(null)
  const [editRating, setEditRating] = useState(5)
  const [editComment, setEditComment] = useState('')

  // Load once when the product changes.
  useEffect(() => {
    let cancelled = false
    setLoading(true)
    reviewWishlistApi
      .getProductReviews(productId)
      .then((list) => {
        if (!cancelled) setReviews(list)
      })
      .catch((err) => {
        if (!cancelled) setError(getErrorMessage(err, 'Could not load reviews.'))
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [productId])

  const myReview = reviews?.find((review) => review.userId === currentUserId) ?? null

  async function refresh() {
    try {
      setReviews(await reviewWishlistApi.getProductReviews(productId))
    } catch {
      /* keep the previous list on a transient failure */
    }
    onReviewsChanged?.()
  }

  async function handleCreate(event: FormEvent) {
    event.preventDefault()
    setFormNotice(null)
    setSubmitting(true)
    try {
      await reviewWishlistApi.addReview(productId, { rating, comment: comment.trim() })
      setComment('')
      setRating(5)
      setFormNotice('Thanks! Your review has been published.')
      await refresh()
    } catch (err) {
      setFormNotice(getErrorMessage(err, 'Could not post your review.'))
    } finally {
      setSubmitting(false)
    }
  }

  async function handleUpdate(event: FormEvent) {
    event.preventDefault()
    if (editingId === null) return
    setFormNotice(null)
    setSubmitting(true)
    try {
      await reviewWishlistApi.updateReview(editingId, {
        rating: editRating,
        comment: editComment.trim(),
      })
      setEditingId(null)
      await refresh()
    } catch (err) {
      setFormNotice(getErrorMessage(err, 'Could not update your review.'))
    } finally {
      setSubmitting(false)
    }
  }

  async function handleDelete(reviewId: number) {
    setFormNotice(null)
    try {
      await reviewWishlistApi.deleteReview(reviewId)
      await refresh()
    } catch (err) {
      setFormNotice(getErrorMessage(err, 'Could not delete your review.'))
    }
  }

  return (
    <section id="reviews" className="mt-14 scroll-mt-24">
      <h2 className="heading-panel uppercase">Customer Reviews</h2>

      {/* ---------------- Write a review ---------------- */}
      <div className="card mt-6 p-6">
        {!isAuthenticated || !isCustomer ? (
          <p className="text-sm text-navy/70">
            Sign in with a customer account to review a product you have received.
          </p>
        ) : myReview ? (
          <>
            <p className="text-sm font-semibold text-royal">Your review</p>
            {editingId === myReview.id ? (
              <form onSubmit={handleUpdate} className="mt-3">
                <StarRatingInput value={editRating} onChange={setEditRating} />
                <textarea
                  className="field mt-3 min-h-24"
                  value={editComment}
                  onChange={(e) => setEditComment(e.target.value)}
                  required
                  maxLength={1000}
                  placeholder="Update your review"
                />
                <div className="mt-3 flex gap-3">
                  <Button type="submit" loading={submitting}>
                    Save changes
                  </Button>
                  <Button type="button" variant="ghost" onClick={() => setEditingId(null)}>
                    Cancel
                  </Button>
                </div>
              </form>
            ) : (
              <>
                <div className="mt-2">
                  <StarRating rating={myReview.rating} size="md" />
                </div>
                <p className="mt-2 whitespace-pre-line text-navy/80">{myReview.comment}</p>
                <div className="mt-3 flex gap-3">
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={() => {
                      setEditingId(myReview.id)
                      setEditRating(myReview.rating)
                      setEditComment(myReview.comment)
                    }}
                  >
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={() => void handleDelete(myReview.id)}
                    aria-label="Delete my review"
                  >
                    <Trash2 className="h-4 w-4" /> Delete
                  </Button>
                </div>
              </>
            )}
          </>
        ) : (
          <form onSubmit={handleCreate}>
            <p className="text-sm font-semibold text-royal">Write a review</p>
            <StarRatingInput value={rating} onChange={setRating} />
            <textarea
              className="field mt-3 min-h-24"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              required
              maxLength={1000}
              placeholder="How did this product work for your skin?"
            />
            <p className="mt-1 text-xs text-navy/50">
              Reviews are only accepted for products you have purchased and received.
            </p>
            <Button type="submit" loading={submitting} className="mt-3">
              Post review
            </Button>
          </form>
        )}

        {formNotice && <p className="mt-3 text-sm text-royal">{formNotice}</p>}
      </div>

      {/* ---------------- Review list ---------------- */}
      <div className="mt-6 space-y-4">
        {loading ? (
          <Spinner label="Loading reviews..." />
        ) : error ? (
          <p className="text-sm text-red-700">{error}</p>
        ) : !reviews || reviews.length === 0 ? (
          <EmptyState
            title="No reviews yet"
            message="Be the first to share how this product worked for your skin."
          />
        ) : (
          reviews.map((review) => (
            <article key={review.id} className="card p-5">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="font-semibold text-navy">{review.userName}</p>
                  <p className="text-xs text-navy/50">{formatDateTime(review.createdAt)}</p>
                </div>
                <StarRating rating={review.rating} />
              </div>
              {review.comment && (
                <p className="mt-3 whitespace-pre-line text-navy/80">{review.comment}</p>
              )}
            </article>
          ))
        )}
      </div>
    </section>
  )
}

/** Accessible 1..5 star picker rendered as radio buttons. */
function StarRatingInput({ value, onChange }: { value: number; onChange: (v: number) => void }) {
  return (
    <fieldset className="mt-3">
      <legend className="field-label">Rating</legend>
      <div className="flex gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <label key={star} className="cursor-pointer">
            <input
              type="radio"
              name="rating"
              value={star}
              checked={value === star}
              onChange={() => onChange(star)}
              className="sr-only"
            />
            <span
              className={`text-2xl ${star <= value ? 'text-yellow-gold' : 'text-navy/25'}`}
              aria-hidden="true"
            >
              ★
            </span>
            <span className="sr-only">{star} star{star > 1 ? 's' : ''}</span>
          </label>
        ))}
      </div>
    </fieldset>
  )
}
