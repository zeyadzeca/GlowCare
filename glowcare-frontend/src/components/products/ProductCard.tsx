import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Heart, ShoppingBag } from 'lucide-react'
import type { ProductListItem } from '@/types/api.types'
import { formatPrice } from '@/utils/format'
import { getErrorMessage } from '@/utils/errors'
import { StarRating } from '@/components/common/StarRating'
import { useAuth } from '@/context/AuthContext'
import { useCart } from '@/context/CartContext'
import { reviewWishlistApi } from '@/services/api'

interface ProductCardProps {
  product: ProductListItem
  isInWishlist?: boolean
  onWishlistChange?: (productId: number, isWishlisted: boolean) => void
}

/**
 * Product card used on the home page, product grid and wishlist.
 *
 * Guest rules (enforced by the backend, mirrored here for UX):
 * - Guests can VIEW the card and open the details page.
 * - Guests CAN add to cart (guest cart), but CANNOT add to wishlist.
 */
export function ProductCard({ product, isInWishlist = false, onWishlistChange }: ProductCardProps) {
  const { isAuthenticated, hasRole } = useAuth()
  const { addItem } = useCart()

  const [busy, setBusy] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)
  const [wishlisted, setWishlisted] = useState(isInWishlist)

  const isCustomer = hasRole('Customer')

  async function handleAddToCart() {
    setNotice(null)
    setBusy(true)
    try {
      await addItem({ productId: product.id, quantity: 1 })
      setNotice('Added to cart.')
    } catch (error) {
      setNotice(getErrorMessage(error))
    } finally {
      setBusy(false)
    }
  }

  async function handleAddToWishlist() {
    setNotice(null)
    if (!isCustomer) {
      setNotice('Please sign in as a customer to save items.')
      return
    }
    setBusy(true)
    try {
      await reviewWishlistApi.addToWishlist(product.id)
      setWishlisted(true)
      onWishlistChange?.(product.id, true)
      setNotice('Saved to wishlist.')
    } catch (error) {
      setNotice(getErrorMessage(error))
    } finally {
      setBusy(false)
    }
  }

  async function handleRemoveFromWishlist() {
    setNotice(null)
    setBusy(true)
    try {
      await reviewWishlistApi.removeFromWishlist(product.id)
      setWishlisted(false)
      onWishlistChange?.(product.id, false)
      setNotice('Removed from wishlist.')
    } catch (error) {
      setNotice(getErrorMessage(error))
    } finally {
      setBusy(false)
    }
  }

  return (
    <article className="group flex flex-col overflow-hidden rounded-2xl bg-white shadow-sm transition-shadow hover:shadow-lg">
      <Link
        to={`/products/${product.id}`}
        className="relative block aspect-square overflow-hidden bg-sky/50"
      >
        {product.mainImageUrl ? (
          <img
            src={product.mainImageUrl}
            alt={product.name}
            loading="lazy"
            onError={(e) => {
              e.currentTarget.style.display = 'none'
            }}
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center">
            <span className="font-display text-4xl text-royal/30">GLOW</span>
          </div>
        )}

        <span className="absolute left-3 top-3 rounded-full bg-white px-3 py-1 text-xs font-semibold text-royal">
          {product.categoryName}
        </span>

        {product.spf !== null && (
          <span className="absolute right-3 top-3 rounded-full bg-yellow px-2 py-1 text-xs font-bold text-royal">
            SPF {product.spf}
          </span>
        )}
      </Link>

      <div className="flex flex-1 flex-col p-4">
        <p className="text-xs font-semibold uppercase tracking-wide text-navy/50">{product.brandName}</p>

        <Link
          to={`/products/${product.id}`}
          className="mt-1 font-display text-lg font-semibold text-royal hover:underline"
        >
          {product.name}
        </Link>

        <div className="mt-1">
          <StarRating rating={product.averageRating} count={product.reviewCount} />
        </div>

        <div className="mt-auto flex items-center justify-between pt-3">
          <span className="font-display text-xl font-bold text-royal">
            {formatPrice(product.price)}
          </span>

          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={wishlisted ? handleRemoveFromWishlist : handleAddToWishlist}
              disabled={busy}
              className="icon-btn"
              aria-label={wishlisted ? 'Remove from wishlist' : isAuthenticated ? 'Save to wishlist' : 'Sign in to save'}
            >
              <Heart className={`h-4 w-4 ${wishlisted ? 'fill-red-500 text-red-500' : ''}`} />
            </button>
            <button
              type="button"
              onClick={handleAddToCart}
              disabled={busy || !product.inStock}
              className="flex h-9 w-9 items-center justify-center rounded-full bg-yellow text-royal transition-colors hover:bg-yellow-deep disabled:cursor-not-allowed disabled:opacity-50"
              aria-label={isAuthenticated ? 'Add to cart' : 'Sign in to buy'}
              title={product.inStock ? 'Add to cart' : 'Out of stock'}
            >
              <ShoppingBag className="h-4 w-4" />
            </button>
          </div>
        </div>

        {!product.inStock && <p className="mt-1 text-xs font-medium text-red-600">Out of stock</p>}
        {notice && <p className="mt-2 text-xs text-royal">{notice}</p>}
      </div>
    </article>
  )
}
