import api from './client'
import type {
  AdminDashboardStats,
  Brand,
  Category,
  Ingredient,
  Order,
  ProductDetails,
  SellerApplication,
  SkinType,
  UserManagement,
} from '@/types/api.types'

/**
 * Admin dashboard endpoints (Admin role only).
 * Category / brand / skin type / ingredient CRUD is shared with the public
 * catalog reads, but writes require the Admin role.
 */

/* --------------------------- Dashboard ---------------------------- */

export async function getAdminDashboard(): Promise<AdminDashboardStats> {
  const { data } = await api.get<AdminDashboardStats>('/admin/dashboard')
  return data
}

/* ----------------------------- Users ------------------------------ */

export async function getAdminUsers(): Promise<UserManagement[]> {
  const { data } = await api.get<UserManagement[]>('/admin/users')
  return data
}

/** PUT /api/admin/users/{id}/role - grants or removes a role. */
export async function updateUserRole(
  userId: string,
  role: string,
  assign: boolean,
): Promise<void> {
  await api.put(`/admin/users/${userId}/role`, { role, assign })
}

/* ----------------------- Seller applications ---------------------- */

export async function getSellerApplications(): Promise<SellerApplication[]> {
  const { data } = await api.get<SellerApplication[]>('/admin/seller-applications')
  return data
}

/** PUT /api/admin/seller-applications/{id}/approval - approve or reject. */
export async function updateSellerApproval(
  sellerProfileId: number,
  isApproved: boolean,
): Promise<SellerApplication> {
  const { data } = await api.put<SellerApplication>(
    `/admin/seller-applications/${sellerProfileId}/approval`,
    { isApproved },
  )
  return data
}

/* ----------------------------- Orders ----------------------------- */

export async function getAllOrders(): Promise<Order[]> {
  const { data } = await api.get<Order[]>('/admin/orders')
  return data
}

/** PUT /api/admin/orders/{id}/status - moves the order through its timeline. */
export async function updateOrderStatus(
  id: number,
  status: number,
  notes?: string | null,
): Promise<Order> {
  const { data } = await api.put<Order>(`/admin/orders/${id}/status`, { status, notes })
  return data
}

/* --------------------------- Products ----------------------------- */

/** DELETE /api/products/{id}/permanent - Admin only, refused if referenced. */
export async function adminDeleteProduct(id: number): Promise<void> {
  await api.delete(`/products/${id}/permanent`)
}

/* ------------------ Taxonomy CRUD (Admin only) --------------------- */

type TaxonomyKind = 'categories' | 'brands' | 'skintypes' | 'ingredients'

export async function createCategory(payload: Partial<Category>): Promise<Category> {
  const { data } = await api.post<Category>('/categories', payload)
  return data
}
export async function updateCategory(id: number, payload: Partial<Category>): Promise<Category> {
  const { data } = await api.put<Category>(`/categories/${id}`, payload)
  return data
}
export async function deleteCategory(id: number): Promise<void> {
  await api.delete(`/categories/${id}`)
}

export async function createBrand(payload: Partial<Brand>): Promise<Brand> {
  const { data } = await api.post<Brand>('/brands', payload)
  return data
}
export async function updateBrand(id: number, payload: Partial<Brand>): Promise<Brand> {
  const { data } = await api.put<Brand>(`/brands/${id}`, payload)
  return data
}
export async function deleteBrand(id: number): Promise<void> {
  await api.delete(`/brands/${id}`)
}

export async function createSkinType(payload: Partial<SkinType>): Promise<SkinType> {
  const { data } = await api.post<SkinType>('/skintypes', payload)
  return data
}
export async function updateSkinType(id: number, payload: Partial<SkinType>): Promise<SkinType> {
  const { data } = await api.put<SkinType>(`/skintypes/${id}`, payload)
  return data
}
export async function deleteSkinType(id: number): Promise<void> {
  await api.delete(`/skintypes/${id}`)
}

export async function createIngredient(payload: Partial<Ingredient>): Promise<Ingredient> {
  const { data } = await api.post<Ingredient>('/ingredients', payload)
  return data
}
export async function updateIngredient(
  id: number,
  payload: Partial<Ingredient>,
): Promise<Ingredient> {
  const { data } = await api.put<Ingredient>(`/ingredients/${id}`, payload)
  return data
}
export async function deleteIngredient(id: number): Promise<void> {
  await api.delete(`/ingredients/${id}`)
}

export type { TaxonomyKind }
export type { ProductDetails }
