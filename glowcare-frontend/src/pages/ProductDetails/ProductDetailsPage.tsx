import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ChevronRight, Heart, Minus, Plus, ShieldCheck, ShoppingBag, Truck } from 'lucide-react'
import { catalogApi, reviewWishlistApi } from '@/services/api'
import { useAuth } from '@/context/AuthContext'
import { useCart } from '@/context/CartContext'
import type { ProductDetails, ProductListItem } from '@/types/api.types'
import { formatPrice } from '@/utils/format'
import { getErrorMessage } from '@/utils/errors'
import { Button } from '@/components/common/Button'
import { ErrorState, Spinner } from '@/components/common/States'
import { StarRating } from '@/components/common/StarRating'
import { EditorialImage } from '@/components/common/EditorialImage'
import { ProductCard } from '@/components/products/ProductCard'
import { ReviewsSection } from '@/components/reviews/ReviewsSection'

/**
 * Product details page (/products/:id).
 *
 * All data comes from the real catalog endpoints:
 *   GET /api/products/{id}          - full details
 *   GET /api/products/{id}/reviews  - review list (via ReviewsSection)
 *   GET /api/products?categoryId=   - related products in the same category
 *
 * Money always comes from the server; quantity is the only thing this page
 * computes, and it is capped by `stockQuantity`.
 */
export function ProductDetailsPage() {
  const { id } = useParams()
  const productId = Number(id)
  const navigate = useNavigate()

  const { isAuthenticated, user, hasRole } = useAuth()
  const { addItem } = useCart()
  const isCustomer = hasRole('Customer')

  const [product, setProduct] = useState<ProductDetails | null>(null)
  const [related, setRelated] = useState<ProductListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [activeImage, setActiveImage] = useState<string | null>(null)
  const [quantity, setQuantity] = useState(1)
  const [busy, setBusy] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!Number.isFinite(productId) || productId <= 0) {
      setError('That product does not exist.')
      setLoading(false)
      return
    }
    setLoading(true)
    setError(null)
    try {
      const details = await catalogApi.getProduct(productId)
      setProduct(details)
      setActiveImage(details.images[0] ?? null)
      setQuantity(1)

      // Related products are a bonus - a failure here must not break the page.
      catalogApi
        .getProducts({ categoryId: details.categoryId, pageSize: 4 })
        .then((result) => setRelated(result.items.filter((item) => item.id !== details.id)))
        .catch(() => setRelated([]))
    } catch (err) {
      setError(getErrorMessage(err, 'Could not load this product.'))
    } finally {
      setLoading(false)
    }
  }, [productId])

  useEffect(() => {
    void load()
  }, [load])

  async function handleAddToCart() {
    setNotice(null)
    if (!isCustomer) {
      setNotice('Please sign in as a customer to add items to your cart.')
      return
    }
    setBusy(true)
    try {
      await addItem({ productId, quantity })
      setNotice(`Added ${quantity} to your cart.`)
    } catch (err) {
      setNotice(getErrorMessage(err))
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
      await reviewWishlistApi.addToWishlist(productId)
      setNotice('Saved to your wishlist.')
    } catch (err) {
      setNotice(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  function handleBuyNow(event: FormEvent) {
    event.preventDefault()
    // Same as add-to-cart, then jump straight to checkout.
    void handleAddToCart().then(() => {
      if (isCustomer) navigate('/checkout')
    })
  }

  /* ---------------------------------------------------------------- states */

  if (loading) return <div className="shell"><Spinner label="Loading product..." /></div>

  if (error || !product) {
    return (
      <div className="shell py-12">
        <ErrorState message={error ?? 'Product not found.'} onRetry={() => void load()} />
        <div className="mt-6 text-center">
          <Link to="/products" className="btn-ghost">
            Back to shop
          </Link>
        </div>
      </div>
    )
  }

  const maxQuantity = Math.max(product.stockQuantity, 0)
  const gallery = product.images.length > 0 ? product.images : [null]

  /* --------------------------------------------------------------- render */

  return (
    <div className="shell py-8 lg:py-12">
      {/* Breadcrumb */}
      <nav aria-label="Breadcrumb" className="flex items-center gap-1 text-sm text-navy/60">
        <Link to="/" className="hover:text-royal">Home</Link>
        <ChevronRight className="h-4 w-4" />
        <Link to="/products" className="hover:text-royal">Shop</Link>
        <ChevronRight className="h-4 w-4" />
        <Link to={`/products?categoryId=${product.categoryId}`} className="hover:text-royal">
          {product.categoryName}
        </Link>
        <ChevronRight className="h-4 w-4" />
        <span className="text-navy">{product.name}</span>
      </nav>

      <div className="mt-6 grid gap-10 lg:grid-cols-2">
        {/* ============================================================
            Gallery - sky-blue frame + thumbnails
        ============================================================ */}
        <div>
          <EditorialImage
            src={activeImage}
            alt={product.name}
            shape="organic"
            ratio="aspect-square"
            className="bg-sky/60"
          />

          {gallery.length > 1 && (
            <div className="mt-4 flex flex-wrap gap-3">
              {gallery.map((image, index) => (
                <button
                  key={`${image}-${index}`}
                  type="button"
                  onClick={() => setActiveImage(image)}
                  className={`h-20 w-20 overflow-hidden rounded-xl border-2 transition-colors ${
                    activeImage === image ? 'border-royal' : 'border-transparent'
                  }`}
                  aria-label={`View image ${index + 1}`}
                >
                  {image ? (
                    <img src={image} alt="" className="h-full w-full object-cover" />
                  ) : (
                    <span className="flex h-full w-full items-center justify-center bg-sky/50 text-xs text-royal">
                      {index + 1}
                    </span>
                  )}
                </button>
              ))}
            </div>
          )}
        </div>

        {/* ============================================================
            Summary
        ============================================================ */}
        <div>
          <p className="text-xs font-semibold uppercase tracking-widest text-navy/50">
            {product.brandName} · {product.categoryName}
          </p>

          <h1 className="mt-2 font-display font-bold uppercase leading-[1.02] text-royal">
            <span className="text-display-sm sm:text-display-md">{product.name}</span>
          </h1>

          <a href="#reviews" className="mt-3 inline-block">
            <StarRating rating={product.averageRating} count={product.reviewCount} size="md" />
          </a>

          <p className="mt-5 font-display text-display-sm text-royal">
            {formatPrice(product.price)}
          </p>

          {/* Stock */}
          <p className="mt-1 text-sm">
            {product.inStock && maxQuantity > 0 ? (
              <span className="font-semibold text-green-700">
                In stock{maxQuantity <= 5 ? ` - only ${maxQuantity} left` : ''}
              </span>
            ) : (
              <span className="font-semibold text-red-600">Out of stock</span>
            )}
          </p>

          <p className="mt-5 text-navy/80">{product.description}</p>

          {/* Attributes */}
          <dl className="mt-6 grid grid-cols-2 gap-3 text-sm">
            {product.volume && (
              <div className="card px-4 py-3">
                <dt className="text-navy/50">Volume</dt>
                <dd className="font-semibold text-navy">{product.volume}</dd>
              </div>
            )}
            {product.spf !== null && (
              <div className="card px-4 py-3">
                <dt className="text-navy/50">SPF</dt>
                <dd className="font-semibold text-navy">{product.spf}</dd>
              </div>
            )}
            <div className="card px-4 py-3">
              <dt className="text-navy/50">Sold by</dt>
              <dd className="font-semibold text-navy">{product.sellerStoreName}</dd>
            </div>
            <div className="card px-4 py-3">
              <dt className="text-navy/50">Skin types</dt>
              <dd className="font-semibold text-navy">
                {product.skinTypes.length > 0 ? product.skinTypes.join(', ') : 'All skin types'}
              </dd>
            </div>
          </dl>

          {/* Ingredients */}
          {product.ingredients.length > 0 && (
            <div className="mt-5">
              <h2 className="text-sm font-semibold text-royal">Key ingredients</h2>
              <div className="mt-2 flex flex-wrap gap-2">
                {product.ingredients.map((ingredient) => (
                  <span
                    key={ingredient}
                    className="badge bg-yellow text-royal"
                  >
                    {ingredient}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Quantity + actions */}
          <div className="mt-7 flex flex-wrap items-center gap-4">
            <div className="flex items-center gap-1 rounded-full border border-navy/15 bg-white p-1">
              <button
                type="button"
                onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                disabled={quantity <= 1}
                className="icon-btn h-8 w-8 disabled:opacity-40"
                aria-label="Decrease quantity"
              >
                <Minus className="h-4 w-4" />
              </button>
              <span className="w-8 text-center font-semibold text-navy" aria-live="polite">
                {quantity}
              </span>
              <button
                type="button"
                onClick={() => setQuantity((q) => Math.min(maxQuantity || 1, q + 1))}
                disabled={quantity >= maxQuantity}
                className="icon-btn h-8 w-8 disabled:opacity-40"
                aria-label="Increase quantity"
              >
                <Plus className="h-4 w-4" />
              </button>
            </div>

            <Button
              onClick={() => void handleAddToCart()}
              loading={busy}
              disabled={!product.inStock}
              className="flex-1 sm:flex-none"
            >
              <ShoppingBag className="h-4 w-4" /> Add to cart
            </Button>

            <Button
              variant="accent"
              onClick={() => void handleAddToWishlist()}
              disabled={busy}
              aria-label={isAuthenticated ? 'Save to wishlist' : 'Sign in to save'}
            >
              <Heart className="h-4 w-4" /> Wishlist
            </Button>
          </div>

          <form onSubmit={handleBuyNow} className="mt-3">
            <Button type="submit" variant="light" disabled={!product.inStock} className="w-full">
              Buy it now
            </Button>
          </form>

          {notice && <p className="mt-3 text-sm text-royal">{notice}</p>}

          {/* Trust row */}
          <div className="mt-6 grid gap-3 border-t border-navy/10 pt-6 text-sm text-navy/70 sm:grid-cols-2">
            <p className="flex items-center gap-2">
              <Truck className="h-4 w-4 text-royal" />
              Free shipping over EGP 500
            </p>
            <p className="flex items-center gap-2">
              <ShieldCheck className="h-4 w-4 text-royal" />
              Verified seller marketplace
            </p>
          </div>
        </div>
      </div>

      {/* ============================================================
          Reviews
      ============================================================ */}
      <ReviewsSection productId={productId} currentUserId={user?.id} />

      {/* ============================================================
          Related products (same category, from the real API)
      ============================================================ */}
      {related.length > 0 && (
        <section className="mt-16">
          <h2 className="heading-panel uppercase">You may also like</h2>
          <div className="mt-6 grid grid-cols-2 gap-5 md:grid-cols-4">
            {related.map((item) => (
              <ProductCard key={item.id} product={item} />
            ))}
          </div>
        </section>
      )}
    </div>
  )
}
