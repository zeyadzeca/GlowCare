import api from './client'
import type {
  CheckoutRequest,
  Order,
  OrderSummary,
  PaymentInitiation,
} from '@/types/api.types'

/**
 * Checkout + order endpoints (Customer role only).
 *
 * Money is ALWAYS calculated by the backend. The frontend only ever sends
 * { addressId, paymentMethod, promoCode } and renders the returned totals.
 */

/** POST /api/orders/preview - cost breakdown for the checkout page. */
export async function previewOrder(payload: CheckoutRequest): Promise<OrderSummary> {
  const { data } = await api.post<OrderSummary>('/orders/preview', payload)
  return data
}

/** POST /api/orders - places a cash-on-delivery order. */
export async function placeCodOrder(payload: CheckoutRequest): Promise<Order> {
  const { data } = await api.post<Order>('/orders', payload)
  return data
}

/** POST /api/payments/initiate - starts a Paymob card checkout. */
export async function initiatePayment(payload: CheckoutRequest): Promise<PaymentInitiation> {
  const { data } = await api.post<PaymentInitiation>('/payments/initiate', payload)
  return data
}

/** GET /api/orders - my order history, newest first. */
export async function getMyOrders(): Promise<Order[]> {
  const { data } = await api.get<Order[]>('/orders')
  return data
}

/** GET /api/orders/{id} - one of my orders with items + status timeline. */
export async function getOrder(id: number): Promise<Order> {
  const { data } = await api.get<Order>(`/orders/${id}`)
  return data
}

/** POST /api/orders/{id}/cancel - cancels a Pending order (restocks items). */
export async function cancelOrder(id: number): Promise<Order> {
  const { data } = await api.post<Order>(`/orders/${id}/cancel`)
  return data
}

/** GET /api/payments/{id} - one of MY payments. */
export async function getPayment(id: number) {
  const { data } = await api.get(`/payments/${id}`)
  return data
}
