/**
 * Enums that mirror the backend `GlowCare.API.Enums` values EXACTLY.
 *
 * ASP.NET Core serialises enums as numbers by default, so the API returns e.g.
 * `"status": 2`. We keep the same numeric values on the frontend and attach
 * human-readable labels separately (see `utils/labels.ts`).
 *
 * NOTE: written as `as const` objects (not TypeScript `enum`) because the
 * project sets `erasableSyntaxOnly`, which forbids runtime-emitting syntax.
 */
export const OrderStatus = {
  Pending: 0,
  Processing: 1,
  Shipped: 2,
  Delivered: 3,
  Cancelled: 4,
} as const
export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus]

export const PaymentMethod = {
  CashOnDelivery: 0,
  CreditCard: 1,
} as const
export type PaymentMethod = (typeof PaymentMethod)[keyof typeof PaymentMethod]

export const PaymentStatus = {
  Pending: 0,
  Paid: 1,
  Failed: 2,
} as const
export type PaymentStatus = (typeof PaymentStatus)[keyof typeof PaymentStatus]

export const FulfillmentStatus = {
  NotFulfilled: 0,
  Fulfilled: 1,
} as const
export type FulfillmentStatus =
  (typeof FulfillmentStatus)[keyof typeof FulfillmentStatus]

export const ApprovalStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
} as const
export type ApprovalStatus = (typeof ApprovalStatus)[keyof typeof ApprovalStatus]

export const DiscountType = {
  Percentage: 0,
  Fixed: 1,
} as const
export type DiscountType = (typeof DiscountType)[keyof typeof DiscountType]

/** Roles exactly as seeded by the backend (`SeedData.Roles`). */
export type Role = 'Admin' | 'Seller' | 'Customer'
