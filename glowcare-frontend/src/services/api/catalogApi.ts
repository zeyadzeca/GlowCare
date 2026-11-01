import api from './client'
import { toQueryString } from '@/utils/format'
import type {
  Brand,
  Category,
  Ingredient,
  PagedResult,
  ProductDetails,
  ProductFilter,
  ProductListItem,
  SkinType,
} from '@/types/api.types'

/**
 * Public catalog endpoints. GETs are anonymous (guests can browse); product
 * writes live in sellerApi because they require the Seller role.
 */

/** GET /api/products - paged, filtered, sorted list of ACTIVE products. */
export async function getProducts(filter: ProductFilter = {}): Promise<PagedResult<ProductListItem>> {
  const { data } = await api.get<PagedResult<ProductListItem>>(
    `/products${toQueryString({ ...filter })}`,
  )
  return data
}

/** GET /api/products/{id} - full details (404 for unknown/inactive). */
export async function getProduct(id: number): Promise<ProductDetails> {
  const { data } = await api.get<ProductDetails>(`/products/${id}`)
  return data
}

export async function getCategories(): Promise<Category[]> {
  const { data } = await api.get<Category[]>('/categories')
  return data
}

export async function getFeaturedCategories(limit = 7): Promise<Category[]> {
  const { data } = await api.get<Category[]>('/categories/featured', { params: { limit } })
  return data
}

export async function getBrands(): Promise<Brand[]> {
  const { data } = await api.get<Brand[]>('/brands')
  return data
}

export async function getFeaturedBrands(limit = 7): Promise<Brand[]> {
  const { data } = await api.get<Brand[]>('/brands/featured', { params: { limit } })
  return data
}

export async function getSkinTypes(): Promise<SkinType[]> {
  const { data } = await api.get<SkinType[]>('/skintypes')
  return data
}

export async function getIngredients(): Promise<Ingredient[]> {
  const { data } = await api.get<Ingredient[]>('/ingredients')
  return data
}
