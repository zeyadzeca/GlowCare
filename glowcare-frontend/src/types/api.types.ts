import type {
  ApprovalStatus,
  DiscountType,
  FulfillmentStatus,
  OrderStatus,
  PaymentMethod,
  PaymentStatus,
  Role,
} from './enums'

/**
 * DTOs that mirror the backend response/request shapes 1:1.
 *
 * IMPORTANT: ASP.NET Core's default JSON serializer uses camelCase, so every
 * property here is camelCased exactly as the API returns it. If the backend
 * DTO changes, this file must be updated to match - never reshape responses on
 * the client instead.
 */

/* ------------------------------------------------------------------ */
/* Common                                                              */
/* ------------------------------------------------------------------ */

/** Standard shape returned by the global ExceptionHandlingMiddleware. */
export interface ApiError {
  statusCode: number
  message: string
}

/** Wrapper returned by every paginated list endpoint (`PagedResultDto<T>`). */
export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  /** Computed server-side: ceil(TotalCount / PageSize). */
  totalPages: number
}

/* ------------------------------------------------------------------ */
/* Auth                                                                */
/* ------------------------------------------------------------------ */

export interface RegisterRequest {
  fullName: string
  email: string
  password: string
  confirmPassword: string
}

export interface LoginRequest {
  email: string
  password: string
}

/** Returned by register + login (`AuthResponseDto`). */
export interface AuthResponse {
  token: string
  expiresAtUtc: string
  userId: string
  email: string
  fullName: string
  roles: Role[]
}

/* ------------------------------------------------------------------ */
/* Users                                                               */
/* ------------------------------------------------------------------ */

export interface UserProfile {
  id: string
  fullName: string
  email: string
  phoneNumber: string | null
  roles: Role[]
}

export interface UpdateProfileRequest {
  fullName: string
  phoneNumber?: string | null
}

export interface Address {
  id: number
  city: string
  street: string
  buildingNumber: string
}

export interface AddressRequest {
  city: string
  street: string
  buildingNumber: string
}

export interface SellerApplicationRequest {
  storeName: string
  description: string
  logo: string
}

export interface SellerProfile {
  id: number
  storeName: string
  description: string
  logo: string
  approvalStatus: ApprovalStatus
  createdAt: string
}

/* ------------------------------------------------------------------ */
/* Catalog                                                             */
/* ------------------------------------------------------------------ */

export interface Category {
  id: number
  name: string
  description: string
  imageUrl: string
}

export interface Brand {
  id: number
  name: string
  description: string
  logo: string
}

export interface SkinType {
  id: number
  name: string
}

export interface Ingredient {
  id: number
  name: string
  description: string
}

/** `ProductListDto` - lightweight card used in grids and carousels. */
export interface ProductListItem {
  id: number
  name: string
  price: number
  brandName: string
  categoryName: string
  mainImageUrl: string | null
  volume: string | null
  spf: number | null
  averageRating: number
  reviewCount: number
  inStock: boolean
}

/** `ProductResponseDto` - full details for the product details page. */
export interface ProductDetails {
  id: number
  name: string
  description: string
  price: number
  stockQuantity: number
  inStock: boolean
  brandId: number
  brandName: string
  categoryId: number
  categoryName: string
  sellerId: string
  sellerStoreName: string
  volume: string
  spf: number | null
  isActive: boolean
  createdAt: string
  images: string[]
  skinTypes: string[]
  ingredients: string[]
  averageRating: number
  reviewCount: number
}

/** Query parameters for `GET /api/products` (`ProductFilterDto`). */
export interface ProductFilter {
  search?: string
  categoryId?: number
  brandId?: number
  skinTypeId?: number
  ingredientId?: number
  minPrice?: number
  maxPrice?: number
  /** "price_asc" | "price_desc" | "newest" | "name" */
  sortBy?: string
  page?: number
  pageSize?: number
}

/** `ProductCreateDto` / `ProductUpdateDto` share the same field set. */
export interface ProductSaveRequest {
  name: string
  description: string
  price: number
  stockQuantity: number
  brandId: number
  categoryId: number
  volume: string
  spf?: number | null
  imageUrls: string[]
  skinTypeIds: number[]
  ingredientIds: number[]
  /** Only sent on update (soft-delete when false). */
  isActive?: boolean
}

/* ------------------------------------------------------------------ */
/* Cart                                                                */
/* ------------------------------------------------------------------ */

export interface CartItem {
  id: number
  productId: number
  productName: string
  brandName: string
  mainImageUrl: string | null
  unitPrice: number
  quantity: number
  /** Computed server-side: UnitPrice * Quantity. */
  lineTotal: number
  /** Current stock so the UI can cap the quantity selector. */
  availableStock: number
}

export interface Cart {
  id: number
  items: CartItem[]
  /** Computed server-side. */
  subtotal: number
  totalItems: number
}

export interface AddCartItemRequest {
  productId: number
  quantity: number
}

/* ------------------------------------------------------------------ */
/* Orders / Checkout                                                   */
/* ------------------------------------------------------------------ */

export interface CheckoutRequest {
  addressId: number
  paymentMethod: PaymentMethod
  promoCode?: string | null
}

/** `OrderSummaryDto` - all money values calculated by the backend. */
export interface OrderSummary {
  subtotal: number
  shippingCost: number
  taxAmount: number
  discountAmount: number
  totalAmount: number
  /** Effective free-shipping threshold from StoreSettings. */
  freeShippingThreshold: number
  appliedPromoCode: string | null
}

export interface OrderItem {
  id: number
  productId: number
  productName: string
  quantity: number
  unitPrice: number
  lineTotal: number
  sellerId: string
  sellerStoreName: string
  productImageUrl: string | null
  fulfillmentStatus: FulfillmentStatus
}

export interface OrderStatusHistoryEntry {
  id: number
  status: OrderStatus
  changedAt: string
  notes: string | null
}

export interface Order {
  id: number
  orderDate: string
  status: OrderStatus
  paymentMethod: PaymentMethod
  paymentStatus: PaymentStatus
  subtotal: number
  shippingCost: number
  taxAmount: number
  discountAmount: number
  totalAmount: number
  shippingAddress: string
  promoCode: string | null
  items: OrderItem[]
  statusHistory: OrderStatusHistoryEntry[]
}

/* ------------------------------------------------------------------ */
/* Payments                                                            */
/* ------------------------------------------------------------------ */

/** `PaymentInitiationDto` - frontend redirects to `paymentUrl`. */
export interface PaymentInitiation {
  orderId: number
  paymentId: number
  paymentUrl: string
}

export interface Payment {
  id: number
  orderId: number
  amount: number
  paymentMethod: PaymentMethod
  paymentStatus: PaymentStatus
  paymentDate: string | null
  transactionId: string | null
}

/* ------------------------------------------------------------------ */
/* Promo codes                                                         */
/* ------------------------------------------------------------------ */

export interface ValidatePromoCodeRequest {
  code: string
  subtotal: number
}

/** `PromoCodeValidationDto` - result of validating a code at checkout. */
export interface PromoCodeValidation {
  isValid: boolean
  message: string
  promoCodeId: number | null
  discountType: DiscountType | null
  discountValue: number | null
  /** Money amount this code would subtract from the subtotal. */
  discountAmount: number
}

export interface PromoCode {
  id: number
  code: string
  discountType: DiscountType
  discountValue: number
  minimumOrderAmount: number
  expiryDate: string
  usageLimit: number
  isActive: boolean
  usageCount: number
}

export interface PromoCodeSaveRequest {
  code: string
  discountType: DiscountType
  discountValue: number
  minimumOrderAmount: number
  expiryDate: string
  usageLimit: number
}

/* ------------------------------------------------------------------ */
/* Reviews                                                             */
/* ------------------------------------------------------------------ */

export interface Review {
  id: number
  productId: number
  userId: string
  userName: string
  rating: number
  comment: string
  createdAt: string
}

export interface ReviewRequest {
  rating: number
  comment: string
}

/** `ReviewSummaryDto` - aggregated rating for cards / details header. */
export interface ReviewSummary {
  averageRating: number
  reviewCount: number
}

/* ------------------------------------------------------------------ */
/* Wishlist                                                            */
/* ------------------------------------------------------------------ */

export interface WishlistItem {
  id: number
  productId: number
  productName: string
  brandName: string
  price: number
  mainImageUrl: string | null
}

export interface Wishlist {
  id: number
  items: WishlistItem[]
}

/* ------------------------------------------------------------------ */
/* Banners                                                             */
/* ------------------------------------------------------------------ */

export interface Banner {
  id: number
  title: string
  imageUrl: string
  link: string
  isActive: boolean
}

export interface BannerSaveRequest {
  title: string
  imageUrl: string
  link: string
  isActive: boolean
}

/* ------------------------------------------------------------------ */
/* Seller                                                              */
/* ------------------------------------------------------------------ */

/** `SellerDashboardStatsDto`. */
export interface SellerDashboardStats {
  totalProducts: number
  activeProducts: number
  totalOrderItems: number
  pendingOrderItems: number
  totalEarnings: number
  confirmedEarnings: number
}

/** `SellerOrderItemDto` - one of the seller's sold items. */
export interface SellerOrderItem {
  orderItemId: number
  orderId: number
  orderDate: string
  orderStatus: OrderStatus
  fulfillmentStatus: FulfillmentStatus
  productId: number
  productName: string
  quantity: number
  unitPrice: number
  lineTotal: number
  shippingAddress: string
  customerName: string
}

/* ------------------------------------------------------------------ */
/* Admin                                                               */
/* ------------------------------------------------------------------ */

/** `AdminDashboardStatsDto`. */
export interface AdminDashboardStats {
  totalUsers: number
  totalSellers: number
  pendingSellerApplications: number
  totalProducts: number
  activeProducts: number
  totalOrders: number
  pendingOrders: number
  totalRevenue: number
  confirmedRevenue: number
}

export interface UserManagement {
  id: string
  fullName: string
  email: string
  phoneNumber: string | null
  roles: Role[]
  isLockedOut: boolean
}

/** `SellerApplicationListDto` - as seen by the admin. */
export interface SellerApplication {
  sellerProfileId: number
  userId: string
  fullName: string
  email: string
  storeName: string
  description: string
  logo: string
  approvalStatus: ApprovalStatus
  createdAt: string
}
