import {
  ApprovalStatus,
  DiscountType,
  FulfillmentStatus,
  OrderStatus,
  PaymentMethod,
  PaymentStatus,
} from '@/types/enums'

/**
 * Human-readable labels for the numeric enums the API returns.
 * Kept in one place so every page renders identical wording.
 */

export const orderStatusLabel: Record<number, string> = {
  [OrderStatus.Pending]: 'Pending',
  [OrderStatus.Processing]: 'Processing',
  [OrderStatus.Shipped]: 'Shipped',
  [OrderStatus.Delivered]: 'Delivered',
  [OrderStatus.Cancelled]: 'Cancelled',
}

/** Tailwind classes per order status for badges. */
export const orderStatusBadge: Record<number, string> = {
  [OrderStatus.Pending]: 'bg-yellow text-royal',
  [OrderStatus.Processing]: 'bg-sky text-navy',
  [OrderStatus.Shipped]: 'bg-royal text-white',
  [OrderStatus.Delivered]: 'bg-green-200 text-green-900',
  [OrderStatus.Cancelled]: 'bg-red-200 text-red-900',
}

export const paymentMethodLabel: Record<number, string> = {
  [PaymentMethod.CashOnDelivery]: 'Cash on Delivery',
  [PaymentMethod.CreditCard]: 'Credit Card',
}

export const paymentStatusLabel: Record<number, string> = {
  [PaymentStatus.Pending]: 'Pending',
  [PaymentStatus.Paid]: 'Paid',
  [PaymentStatus.Failed]: 'Failed',
}

export const paymentStatusBadge: Record<number, string> = {
  [PaymentStatus.Pending]: 'bg-yellow text-royal',
  [PaymentStatus.Paid]: 'bg-green-200 text-green-900',
  [PaymentStatus.Failed]: 'bg-red-200 text-red-900',
}

export const fulfillmentStatusLabel: Record<number, string> = {
  [FulfillmentStatus.NotFulfilled]: 'Not fulfilled',
  [FulfillmentStatus.Fulfilled]: 'Fulfilled',
}

export const approvalStatusLabel: Record<number, string> = {
  [ApprovalStatus.Pending]: 'Pending',
  [ApprovalStatus.Approved]: 'Approved',
  [ApprovalStatus.Rejected]: 'Rejected',
}

export const approvalStatusBadge: Record<number, string> = {
  [ApprovalStatus.Pending]: 'bg-yellow text-royal',
  [ApprovalStatus.Approved]: 'bg-green-200 text-green-900',
  [ApprovalStatus.Rejected]: 'bg-red-200 text-red-900',
}

export const discountTypeLabel: Record<number, string> = {
  [DiscountType.Percentage]: 'Percentage',
  [DiscountType.Fixed]: 'Fixed amount',
}

/** Catalog sort options accepted by the backend `sortBy` filter. */
export const sortOptions = [
  { value: 'newest', label: 'Newest' },
  { value: 'price_asc', label: 'Price: Low to High' },
  { value: 'price_desc', label: 'Price: High to Low' },
  { value: 'name', label: 'Name: A-Z' },
] as const
