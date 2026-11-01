import { Link } from 'react-router-dom'
import { useWishlist } from '@/context/WishlistContext'
import { formatPrice } from '@/utils/format'
import { getErrorMessage } from '@/utils/errors'
import { EmptyState, Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { useState } from 'react'

export function WishlistPage() {
  const { items, loading, isPending, remove } = useWishlist()
  const [error, setError] = useState<string | null>(null)

  async function handleRemove(productId: number) {
    setError(null)
    try {
      await remove(productId)
    } catch (err) {
      setError(getErrorMessage(err))
    }
  }

  if (loading) return <div className="shell"><Spinner label="Loading wishlist..." /></div>

  return (
    <div className="shell py-10">
      <h1 className="heading-display uppercase">Wishlist</h1>
      <p className="mt-2 text-navy/60">{items.length} saved product{items.length === 1 ? '' : 's'}</p>
      {error && <p className="mt-3 text-sm text-red-700">{error}</p>}

      {items.length === 0 ? (
        <div className="mt-8">
          <EmptyState
            title="Nothing saved yet"
            message="Tap the heart on a product to keep it here."
            actionLabel="Browse products"
            actionTo="/products"
          />
        </div>
      ) : (
        <ul className="mt-8 grid grid-cols-2 gap-5 md:grid-cols-3 lg:grid-cols-4">
          {items.map((item) => (
            <li key={item.productId} className="card overflow-hidden">
              <Link to={`/products/${item.productId}`} className="block aspect-square bg-sky/40">
                {item.mainImageUrl ? (
                  <img src={item.mainImageUrl} alt={item.productName} className="h-full w-full object-cover" />
                ) : (
                  <span className="flex h-full items-center justify-center font-display text-royal/40">GLOW</span>
                )}
              </Link>
              <div className="p-4">
                <p className="text-xs uppercase tracking-wide text-navy/50">{item.brandName}</p>
                <Link to={`/products/${item.productId}`} className="font-display text-lg text-royal hover:underline">
                  {item.productName}
                </Link>
                <p className="mt-1 font-semibold text-royal">{formatPrice(item.price)}</p>
                <Button
                  variant="ghost"
                  className="mt-3 w-full py-2 text-sm"
                  disabled={isPending(item.productId)}
                  onClick={() => void handleRemove(item.productId)}
                >
                  Remove
                </Button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
