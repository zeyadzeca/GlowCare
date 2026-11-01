import api from './client'
import type {
  ProductDetails,
  ProductSaveRequest,
  SellerDashboardStats,
  SellerOrderItem,
} from '@/types/api.types'

/**
 * Seller dashboard endpoints (Seller role only). The sellerId always comes from
 * JWT claims on the backend, so a seller can only ever see their own data.
 */

/** GET /api/seller/dashboard - product counts, pending items, earnings. */
export async function getSellerDashboard(): Promise<SellerDashboardStats> {
  const { data } = await api.get<SellerDashboardStats>('/seller/dashboard')
  return data
}

/** GET /api/seller/products - MY products (including inactive), newest first. */
export async function getMyProducts(): Promise<ProductDetails[]> {
  const { data } = await api.get<ProductDetails[]>('/seller/products')
  return data
}

/** GET /api/seller/order-items - items containing MY products, newest first. */
export async function getSellerOrderItems(): Promise<SellerOrderItem[]> {
  const { data } = await api.get<SellerOrderItem[]>('/seller/order-items')
  return data
}

/** PUT /api/seller/order-items/{id}/fulfillment - marks one item fulfilled. */
export async function updateFulfillment(
  orderItemId: number,
  fulfilled: boolean,
): Promise<SellerOrderItem> {
  const { data } = await api.put<SellerOrderItem>(
    `/seller/order-items/${orderItemId}/fulfillment`,
    { fulfilled },
  )
  return data
}

/* -------------------- Seller product management -------------------- */

/** POST /api/products - creates a product for the authenticated seller. */
export async function createProduct(payload: ProductSaveRequest): Promise<ProductDetails> {
  const { data } = await api.post<ProductDetails>('/products', payload)
  return data
}

/** PUT /api/products/{id} - updates a product the caller owns. */
export async function updateProduct(id: number, payload: ProductSaveRequest): Promise<ProductDetails> {
  const { data } = await api.put<ProductDetails>(`/products/${id}`, payload)
  return data
}

/** DELETE /api/products/{id} - soft-delete (deactivates) the product. */
export async function deactivateProduct(id: number): Promise<void> {
  await api.delete(`/products/${id}`)
}

/** POST /api/products/{id}/activate - reactivates a deactivated product. */
export async function activateProduct(id: number): Promise<void> {
  await api.post(`/products/${id}/activate`)
}
