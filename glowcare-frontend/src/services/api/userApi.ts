import api from './client'
import type {
  Address,
  AddressRequest,
  SellerApplicationRequest,
  SellerProfile,
  UpdateProfileRequest,
  UserProfile,
} from '@/types/api.types'

/**
 * The authenticated user's own profile, addresses and seller application.
 * The userId always comes from JWT claims on the backend, so these endpoints
 * can only ever touch the caller's own data.
 */

export async function getProfile(): Promise<UserProfile> {
  const { data } = await api.get<UserProfile>('/users/me')
  return data
}

export async function updateProfile(payload: UpdateProfileRequest): Promise<UserProfile> {
  const { data } = await api.put<UserProfile>('/users/me', payload)
  return data
}

export async function getAddresses(): Promise<Address[]> {
  const { data } = await api.get<Address[]>('/users/me/addresses')
  return data
}

export async function addAddress(payload: AddressRequest): Promise<Address> {
  const { data } = await api.post<Address>('/users/me/addresses', payload)
  return data
}

export async function updateAddress(id: number, payload: AddressRequest): Promise<Address> {
  const { data } = await api.put<Address>(`/users/me/addresses/${id}`, payload)
  return data
}

export async function deleteAddress(id: number): Promise<void> {
  await api.delete(`/users/me/addresses/${id}`)
}

export async function getPhoneNumbers(): Promise<{ id: number; phoneNumber: string; createdAt: string }[]> {
  const { data } = await api.get('/users/me/phone-numbers')
  return data
}

export async function addPhoneNumber(payload: { phoneNumber: string }): Promise<{ id: number; phoneNumber: string; createdAt: string }> {
  const { data } = await api.post('/users/me/phone-numbers', payload)
  return data
}

export async function updatePhoneNumber(id: number, payload: { phoneNumber: string }): Promise<{ id: number; phoneNumber: string; createdAt: string }> {
  const { data } = await api.put(`/users/me/phone-numbers/${id}`, payload)
  return data
}

export async function deletePhoneNumber(id: number): Promise<void> {
  await api.delete(`/users/me/phone-numbers/${id}`)
}

/** POST /users/me/seller-application - creates a Pending SellerProfile. */
export async function applyForSeller(payload: SellerApplicationRequest): Promise<SellerProfile> {
  const { data } = await api.post<SellerProfile>('/users/me/seller-application', payload)
  return data
}

/** GET /users/me/seller-profile - 404 when the user never applied. */
export async function getSellerProfile(): Promise<SellerProfile | null> {
  try {
    const { data } = await api.get<SellerProfile>('/users/me/seller-profile')
    return data
  } catch (error) {
    // 404 simply means "no seller profile yet" - not an error for the UI.
    if (
      typeof error === 'object' &&
      error !== null &&
      'response' in error &&
      (error as { response?: { status?: number } }).response?.status === 404
    ) {
      return null
    }
    throw error
  }
}

/** PUT /users/me/seller-profile - update store name, description, logo. */
export async function updateSellerProfile(payload: { storeName: string; description: string; logo: string }): Promise<SellerProfile> {
  const { data } = await api.put<SellerProfile>('/users/me/seller-profile', payload)
  return data
}

/** POST /users/me/change-password */
export async function changePassword(payload: { currentPassword: string; newPassword: string }): Promise<void> {
  await api.post('/users/me/change-password', payload)
}
