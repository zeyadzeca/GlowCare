import api from './client'
import type {
  Banner,
  BannerSaveRequest,
  PromoCode,
  PromoCodeSaveRequest,
  PromoCodeValidation,
  ValidatePromoCodeRequest,
} from '@/types/api.types'

/* ------------------------------ Banners ----------------------------- */

/** GET /api/banners - only ACTIVE banners (public home-page carousel). */
export async function getActiveBanners(): Promise<Banner[]> {
  const { data } = await api.get<Banner[]>('/banners')
  return data
}

/** GET /api/banners/all - Admin only (includes inactive). */
export async function getAllBanners(): Promise<Banner[]> {
  const { data } = await api.get<Banner[]>('/banners/all')
  return data
}

export async function createBanner(payload: BannerSaveRequest): Promise<Banner> {
  const { data } = await api.post<Banner>('/banners', payload)
  return data
}

export async function updateBanner(id: number, payload: BannerSaveRequest): Promise<Banner> {
  const { data } = await api.put<Banner>(`/banners/${id}`, payload)
  return data
}

export async function deleteBanner(id: number): Promise<void> {
  await api.delete(`/banners/${id}`)
}

/* ----------------------------- Promo codes -------------------------- */

/** POST /api/promocodes/validate - Customer role; discount computed server-side. */
export async function validatePromoCode(
  payload: ValidatePromoCodeRequest,
): Promise<PromoCodeValidation> {
  const { data } = await api.post<PromoCodeValidation>('/promocodes/validate', payload)
  return data
}

/** GET /api/promocodes - Admin only, includes usage counts. */
export async function getPromoCodes(): Promise<PromoCode[]> {
  const { data } = await api.get<PromoCode[]>('/promocodes')
  return data
}

export async function createPromoCode(payload: PromoCodeSaveRequest): Promise<PromoCode> {
  const { data } = await api.post<PromoCode>('/promocodes', payload)
  return data
}

export async function updatePromoCode(id: number, payload: PromoCodeSaveRequest): Promise<PromoCode> {
  const { data } = await api.put<PromoCode>(`/promocodes/${id}`, payload)
  return data
}

/** Deactivates a code (soft delete - order history keeps its FK). */
export async function deactivatePromoCode(id: number): Promise<void> {
  await api.delete(`/promocodes/${id}`)
}
