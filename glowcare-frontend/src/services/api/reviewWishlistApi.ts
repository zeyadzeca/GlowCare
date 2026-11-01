import api from './client'
import type { Review, ReviewRequest, ReviewSummary, Wishlist, WishlistItem } from '@/types/api.types'

/**
 * Reviews and wishlist.
 * - Review creation requires the Customer role AND a verified purchase
 *   (both enforced server-side).
 * - Wishlist endpoints require the Customer role; there is no guest wishlist.
 */

/* ----------------------------- Reviews ----------------------------- */

/** GET /api/products/{id}/reviews - anonymous, newest first. */
export async function getProductReviews(productId: number): Promise<Review[]> {
  const { data } = await api.get<Review[]>(`/products/${productId}/reviews`)
  return data
}

/** GET /api/products/{id}/rating - anonymous aggregate. */
export async function getProductRating(productId: number): Promise<ReviewSummary> {
  const { data } = await api.get<ReviewSummary>(`/products/${productId}/rating`)
  return data
}

/** POST /api/products/{id}/reviews - verified purchasers only, one per product. */
export async function addReview(productId: number, payload: ReviewRequest): Promise<Review> {
  const { data } = await api.post<Review>(`/products/${productId}/reviews`, payload)
  return data
}

/** PUT /api/reviews/{id} - edit MY review. */
export async function updateReview(id: number, payload: ReviewRequest): Promise<Review> {
  const { data } = await api.put<Review>(`/reviews/${id}`, payload)
  return data
}

/** DELETE /api/reviews/{id} - delete MY review. */
export async function deleteReview(id: number): Promise<void> {
  await api.delete(`/reviews/${id}`)
}

/* ----------------------------- Wishlist ---------------------------- */

/** GET /api/wishlist - my full wishlist. */
export async function getWishlist(): Promise<Wishlist> {
  const { data } = await api.get<Wishlist>('/wishlist')
  return data
}

/** POST /api/wishlist/items - saving a product twice is a no-op. */
export async function addToWishlist(productId: number): Promise<Wishlist> {
  const { data } = await api.post<Wishlist>('/wishlist/items', { productId })
  return data
}

/** DELETE /api/wishlist/items/{itemId} - removes one saved product. */
export async function removeFromWishlist(itemId: number): Promise<void> {
  await api.delete(`/wishlist/items/${itemId}`)
}

export type { WishlistItem }
