import api from './client'
import type { AddCartItemRequest, Cart } from '@/types/api.types'

/**
 * Cart endpoints. All require the Customer role - there is no guest cart.
 * Every mutation returns the FULL updated cart, so the UI can simply replace
 * its local state with the response (no manual recalculation).
 */

/** GET /api/cart - the caller's cart with a server-calculated subtotal. */
export async function getCart(): Promise<Cart> {
  const { data } = await api.get<Cart>('/cart')
  return data
}

/** POST /api/cart/items - adds a product, or increases its quantity. */
export async function addToCart(payload: AddCartItemRequest): Promise<Cart> {
  const { data } = await api.post<Cart>('/cart/items', payload)
  return data
}

/** PUT /api/cart/items/{id} - sets a new quantity for one of MY items. */
export async function updateCartItem(cartItemId: number, quantity: number): Promise<Cart> {
  const { data } = await api.put<Cart>(`/cart/items/${cartItemId}`, { quantity })
  return data
}

/** DELETE /api/cart/items/{id} - removes one item. */
export async function removeCartItem(cartItemId: number): Promise<void> {
  await api.delete(`/cart/items/${cartItemId}`)
}

/** DELETE /api/cart - empties the cart. */
export async function clearCart(): Promise<void> {
  await api.delete('/cart')
}
