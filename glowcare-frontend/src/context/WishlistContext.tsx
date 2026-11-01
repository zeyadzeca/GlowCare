import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import type { ProductDetails, ProductListItem, WishlistItem } from '@/types/api.types'
import { reviewWishlistApi } from '@/services/api'
import { useAuth } from './AuthContext'

const GUEST_WISHLIST_KEY = 'glowcare_guest_wishlist'

export type WishlistSnapshot = Pick<
  WishlistItem,
  'productId' | 'productName' | 'brandName' | 'price' | 'mainImageUrl'
>

interface WishlistContextValue {
  items: WishlistItem[]
  loading: boolean
  isGuest: boolean
  isInWishlist: (productId: number) => boolean
  isPending: (productId: number) => boolean
  toggle: (snapshot: WishlistSnapshot) => Promise<void>
  remove: (productId: number) => Promise<void>
  refresh: () => Promise<void>
  mergeGuestWishlist: () => Promise<void>
}

const WishlistContext = createContext<WishlistContextValue | undefined>(undefined)

function readGuestWishlist(): WishlistItem[] {
  try {
    const stored = localStorage.getItem(GUEST_WISHLIST_KEY)
    if (!stored) return []
    const parsed = JSON.parse(stored) as WishlistSnapshot[]
    if (!Array.isArray(parsed)) return []
    const seen = new Set<number>()
    return parsed
      .filter((item) => typeof item?.productId === 'number' && !seen.has(item.productId) && seen.add(item.productId))
      .map((item) => ({
        id: item.productId,
        productId: item.productId,
        productName: item.productName ?? 'Product',
        brandName: item.brandName ?? '',
        price: item.price ?? 0,
        mainImageUrl: item.mainImageUrl ?? null,
      }))
  } catch {
    return []
  }
}

function writeGuestWishlist(items: WishlistItem[]) {
  try {
    localStorage.setItem(
      GUEST_WISHLIST_KEY,
      JSON.stringify(
        items.map((item) => ({
          productId: item.productId,
          productName: item.productName,
          brandName: item.brandName,
          price: item.price,
          mainImageUrl: item.mainImageUrl,
        })),
      ),
    )
  } catch {
    // Ignore quota / private-mode failures.
  }
}

function clearGuestWishlist() {
  try {
    localStorage.removeItem(GUEST_WISHLIST_KEY)
  } catch {
    // Ignore localStorage errors
  }
}

function snapshotFromProduct(product: ProductListItem | ProductDetails): WishlistSnapshot {
  return {
    productId: product.id,
    productName: product.name,
    brandName: product.brandName,
    price: product.price,
    mainImageUrl: 'mainImageUrl' in product ? product.mainImageUrl : product.images[0] ?? null,
  }
}

export function toWishlistSnapshot(product: ProductListItem | ProductDetails): WishlistSnapshot {
  return snapshotFromProduct(product)
}

export function WishlistProvider({ children }: { children: ReactNode }) {
  const { user, isAuthenticated } = useAuth()
  const [items, setItems] = useState<WishlistItem[]>([])
  const [loading, setLoading] = useState(false)
  const [pendingIds, setPendingIds] = useState<number[]>([])
  const pendingRef = useRef(new Set<number>())
  const itemsRef = useRef<WishlistItem[]>([])
  itemsRef.current = items

  const isCustomer = user?.roles.includes('Customer') ?? false
  const isGuest = !isAuthenticated

  const refresh = useCallback(async () => {
    if (!isAuthenticated || !isCustomer) {
      setItems(isGuest ? readGuestWishlist() : [])
      return
    }

    setLoading(true)
    try {
      const wishlist = await reviewWishlistApi.getWishlist()
      setItems(wishlist.items)
    } catch {
      setItems([])
    } finally {
      setLoading(false)
    }
  }, [isAuthenticated, isCustomer, isGuest])

  useEffect(() => {
    void refresh()
  }, [refresh])

  const beginPending = (productId: number): boolean => {
    if (pendingRef.current.has(productId)) return false
    pendingRef.current.add(productId)
    setPendingIds([...pendingRef.current])
    return true
  }

  const endPending = (productId: number) => {
    pendingRef.current.delete(productId)
    setPendingIds([...pendingRef.current])
  }

  const persistGuest = (next: WishlistItem[]) => {
    setItems(next)
    writeGuestWishlist(next)
  }

  const toggle = useCallback(
    async (snapshot: WishlistSnapshot) => {
      if (!beginPending(snapshot.productId)) return

      const previous = itemsRef.current
      const alreadySaved = previous.some((item) => item.productId === snapshot.productId)

      const optimistic = alreadySaved
        ? previous.filter((item) => item.productId !== snapshot.productId)
        : [
            ...previous,
            {
              id: snapshot.productId,
              productId: snapshot.productId,
              productName: snapshot.productName,
              brandName: snapshot.brandName,
              price: snapshot.price,
              mainImageUrl: snapshot.mainImageUrl,
            },
          ]

      try {
        if (isGuest) {
          persistGuest(optimistic)
          return
        }

        if (!isCustomer) {
          throw new Error('Please sign in as a customer to save items.')
        }

        setItems(optimistic)

        if (alreadySaved) {
          await reviewWishlistApi.removeProductFromWishlist(snapshot.productId)
          await refresh()
        } else {
          const wishlist = await reviewWishlistApi.addToWishlist(snapshot.productId)
          setItems(wishlist.items)
        }
      } catch (error) {
        if (isGuest) persistGuest(previous)
        else setItems(previous)
        throw error
      } finally {
        endPending(snapshot.productId)
      }
    },
    [isCustomer, isGuest, refresh],
  )

  const remove = useCallback(
    async (productId: number) => {
      const item = itemsRef.current.find((entry) => entry.productId === productId || entry.id === productId)
      if (!item) return
      await toggle({
        productId: item.productId,
        productName: item.productName,
        brandName: item.brandName,
        price: item.price,
        mainImageUrl: item.mainImageUrl,
      })
    },
    [toggle],
  )

  const mergeGuestWishlist = useCallback(async () => {
    if (!isAuthenticated || !isCustomer) return

    const guestItems = readGuestWishlist()
    if (guestItems.length === 0) {
      clearGuestWishlist()
      await refresh()
      return
    }

    try {
      const wishlist = await reviewWishlistApi.getWishlist()
      const existingProductIds = new Set(wishlist.items.map((item) => item.productId))
      const newItems = guestItems.filter((item) => !existingProductIds.has(item.productId))

      for (const item of newItems) {
        await reviewWishlistApi.addToWishlist(item.productId)
      }
      clearGuestWishlist()
      await refresh()
    } catch (error) {
      console.error('Failed to merge guest wishlist:', error)
    }
  }, [isAuthenticated, isCustomer, refresh])

  const itemIds = useMemo(() => new Set(items.map((item) => item.productId)), [items])
  const pendingSet = useMemo(() => new Set(pendingIds), [pendingIds])

  const value = useMemo<WishlistContextValue>(
    () => ({
      items,
      loading,
      isGuest,
      isInWishlist: (productId) => itemIds.has(productId),
      isPending: (productId) => pendingSet.has(productId),
      toggle,
      remove,
      refresh,
      mergeGuestWishlist,
    }),
    [items, loading, isGuest, itemIds, pendingSet, toggle, remove, refresh, mergeGuestWishlist],
  )

  return <WishlistContext.Provider value={value}>{children}</WishlistContext.Provider>
}

export function useWishlist(): WishlistContextValue {
  const context = useContext(WishlistContext)
  if (context === undefined) {
    throw new Error('useWishlist must be used within a WishlistProvider')
  }
  return context
}
