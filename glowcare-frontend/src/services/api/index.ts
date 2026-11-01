/**
 * Barrel that re-exports every API module.
 * Pages import from here (e.g. `import { getProducts } from '@/services/api'`)
 * so the import surface stays tidy and swapping an implementation later only
 * touches this folder.
 */
export * as authApi from './authApi'
export * as catalogApi from './catalogApi'
export * as cartApi from './cartApi'
export * as orderApi from './orderApi'
export * as userApi from './userApi'
export * as reviewWishlistApi from './reviewWishlistApi'
export * as promoBannerApi from './promoBannerApi'
export * as sellerApi from './sellerApi'
export * as adminApi from './adminApi'

export { default as api, getToken, setToken, clearToken, TOKEN_STORAGE_KEY } from './client'
