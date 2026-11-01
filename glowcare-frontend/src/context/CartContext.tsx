import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import type { AddCartItemRequest, Cart } from '@/types/api.types'
import { cartApi } from '@/services/api'
import { useAuth } from './AuthContext'

const GUEST_CART_KEY = 'glowcare_guest_cart'

interface GuestCartItem {
  productId: number
  quantity: number
}

/**
 * CartContext supports both guest (localStorage) and authenticated (backend) carts.
 *
 * Rules:
 * - Guests use localStorage for cart persistence
 * - Authenticated customers use the backend API
 * - On login/register, guest cart is merged with backend cart
 * - Checkout is restricted to authenticated users only
 */
interface CartContextValue {
  cart: Cart | null
  loading: boolean
  itemCount: number
  isGuest: boolean
  addItem: (payload: AddCartItemRequest) => Promise<void>
  updateItem: (cartItemId: number, quantity: number) => Promise<void>
  removeItem: (cartItemId: number) => Promise<void>
  clear: () => Promise<void>
  refresh: () => Promise<void>
  mergeGuestCart: () => Promise<void>
}

const EMPTY_CART: Cart = { id: 0, items: [], subtotal: 0, totalItems: 0 }

const CartContext = createContext<CartContextValue | undefined>(undefined)

function getGuestCart(): GuestCartItem[] {
  try {
    const stored = localStorage.getItem(GUEST_CART_KEY)
    return stored ? JSON.parse(stored) : []
  } catch {
    return []
  }
}

function setGuestCart(items: GuestCartItem[]) {
  try {
    localStorage.setItem(GUEST_CART_KEY, JSON.stringify(items))
  } catch {
    // Ignore localStorage errors
  }
}

function clearGuestCart() {
  try {
    localStorage.removeItem(GUEST_CART_KEY)
  } catch {
    // Ignore localStorage errors
  }
}

export function CartProvider({ children }: { children: ReactNode }) {
  const { user, isAuthenticated } = useAuth()
  const [cart, setCart] = useState<Cart | null>(null)
  const [loading, setLoading] = useState(false)

  const isCustomer = user?.roles.includes('Customer') ?? false
  const isGuest = !isAuthenticated

  const refresh = useCallback(async () => {
    if (!isAuthenticated || !isCustomer) {
      // Guest cart - load from localStorage
      const guestItems = getGuestCart()
      const subtotal = 0 // Guest carts don't have real pricing
      setCart({
        id: 0,
        items: guestItems.map((item, idx) => ({
          id: idx,
          productId: item.productId,
          productName: 'Product',
          brandName: '',
          mainImageUrl: '',
          quantity: item.quantity,
          unitPrice: 0,
          totalPrice: 0,
          lineTotal: 0,
          availableStock: 0,
        })),
        subtotal,
        totalItems: guestItems.reduce((sum, item) => sum + item.quantity, 0),
      })
      return
    }

    setLoading(true)
    try {
      setCart(await cartApi.getCart())
    } catch {
      setCart(null)
    } finally {
      setLoading(false)
    }
  }, [isAuthenticated, isCustomer])

  useEffect(() => {
    void refresh()
  }, [refresh])

  const addItem = useCallback(async (payload: AddCartItemRequest) => {
    if (isGuest) {
      const guestItems = getGuestCart()
      const existing = guestItems.find((item) => item.productId === payload.productId)
      if (existing) {
        existing.quantity += payload.quantity
      } else {
        guestItems.push({ productId: payload.productId, quantity: payload.quantity })
      }
      setGuestCart(guestItems)
      await refresh()
      return
    }

    const updated = await cartApi.addToCart(payload)
    setCart(updated)
  }, [isGuest, refresh])

  const updateItem = useCallback(async (cartItemId: number, quantity: number) => {
    if (isGuest) {
      const guestItems = getGuestCart()
      const item = guestItems[cartItemId]
      if (item) {
        if (quantity <= 0) {
          guestItems.splice(cartItemId, 1)
        } else {
          item.quantity = quantity
        }
        setGuestCart(guestItems)
        await refresh()
      }
      return
    }

    const updated = await cartApi.updateCartItem(cartItemId, quantity)
    setCart(updated)
  }, [isGuest, refresh])

  const removeItem = useCallback(async (cartItemId: number) => {
    if (isGuest) {
      const guestItems = getGuestCart()
      guestItems.splice(cartItemId, 1)
      setGuestCart(guestItems)
      await refresh()
      return
    }

    await cartApi.removeCartItem(cartItemId)
    setCart(await cartApi.getCart())
  }, [isGuest, refresh])

  const clear = useCallback(async () => {
    if (isGuest) {
      clearGuestCart()
      setCart({ ...EMPTY_CART })
      return
    }

    await cartApi.clearCart()
    setCart({ ...EMPTY_CART })
  }, [isGuest])

  const mergeGuestCart = useCallback(async () => {
    if (!isAuthenticated || !isCustomer) return

    const guestItems = getGuestCart()
    if (guestItems.length === 0) {
      clearGuestCart()
      return
    }

    try {
      // Get current backend cart
      const backendCart = await cartApi.getCart()

      // Merge guest items with backend cart
      for (const guestItem of guestItems) {
        const existing = backendCart.items.find((item) => item.productId === guestItem.productId)
        if (existing) {
          await cartApi.updateCartItem(existing.id, existing.quantity + guestItem.quantity)
        } else {
          await cartApi.addToCart({ productId: guestItem.productId, quantity: guestItem.quantity })
        }
      }

      // Clear guest cart after successful merge
      clearGuestCart()
      await refresh()
    } catch (error) {
      console.error('Failed to merge guest cart:', error)
      // Keep guest cart if merge fails
    }
  }, [isAuthenticated, isCustomer, refresh])

  const itemCount = useMemo(
    () => cart?.items.reduce((total, item) => total + item.quantity, 0) ?? 0,
    [cart],
  )

  const value = useMemo<CartContextValue>(
    () => ({
      cart,
      loading,
      itemCount,
      isGuest,
      addItem,
      updateItem,
      removeItem,
      clear,
      refresh,
      mergeGuestCart,
    }),
    [cart, loading, itemCount, isGuest, addItem, updateItem, removeItem, clear, refresh, mergeGuestCart],
  )

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}

export function useCart(): CartContextValue {
  const context = useContext(CartContext)
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider')
  }
  return context
}
