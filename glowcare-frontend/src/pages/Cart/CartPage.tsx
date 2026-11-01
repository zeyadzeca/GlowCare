import { Link } from 'react-router-dom'
import { Minus, Plus, Trash2 } from 'lucide-react'
import { useCart } from '@/context/CartContext'
import { formatPrice } from '@/utils/format'
import { getErrorMessage } from '@/utils/errors'
import { EmptyState, Spinner } from '@/components/common/States'
import { useState } from 'react'

export function CartPage() {
  const { cart, loading, updateItem, removeItem } = useCart()
  const [notice, setNotice] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  async function changeQuantity(itemId: number, quantity: number, max: number) {
    const next = Math.min(max, Math.max(1, quantity))
    setBusyId(itemId)
    setNotice(null)
    try {
      await updateItem(itemId, next)
    } catch (error) {
      setNotice(getErrorMessage(error))
    } finally {
      setBusyId(null)
    }
  }

  async function remove(itemId: number) {
    setBusyId(itemId)
    setNotice(null)
    try {
      await removeItem(itemId)
    } catch (error) {
      setNotice(getErrorMessage(error))
    } finally {
      setBusyId(null)
    }
  }

  if (loading && !cart) return <div className="shell"><Spinner label="Loading your cart..." /></div>

  if (!cart || cart.items.length === 0) {
    return (
      <div className="shell py-12">
        <h1 className="heading-display uppercase">Your cart</h1>
        <div className="mt-8">
          <EmptyState
            title="Cart is empty"
            message="Sign in as a customer and add products to start an order."
            actionLabel="Continue shopping"
            actionTo="/products"
          />
        </div>
      </div>
    )
  }

  return (
    <div className="shell py-10">
      <h1 className="heading-display uppercase">Your cart</h1>
      <p className="mt-2 text-navy/60">{cart.totalItems} item{cart.totalItems === 1 ? '' : 's'}</p>
      {notice && <p className="mt-4 text-sm text-red-700">{notice}</p>}

      <div className="mt-8 grid gap-8 lg:grid-cols-[1fr_20rem]">
        <ul className="space-y-4">
          {cart.items.map((item) => (
            <li key={item.id} className="card flex flex-col gap-4 p-4 sm:flex-row sm:items-center">
              <Link to={`/products/${item.productId}`} className="h-24 w-24 shrink-0 overflow-hidden rounded-xl bg-sky/40">
                {item.mainImageUrl ? (
                  <img src={item.mainImageUrl} alt="" className="h-full w-full object-cover" />
                ) : (
                  <span className="flex h-full items-center justify-center font-display text-royal/40">GLOW</span>
                )}
              </Link>
              <div className="min-w-0 flex-1">
                <p className="text-xs uppercase tracking-wide text-navy/50">{item.brandName}</p>
                <Link to={`/products/${item.productId}`} className="font-display text-lg text-royal hover:underline">
                  {item.productName}
                </Link>
                <p className="mt-1 text-sm text-navy/70">{formatPrice(item.unitPrice)} each</p>
              </div>
              <div className="flex items-center gap-3">
                <div className="flex items-center gap-1 rounded-full border border-navy/15 bg-white p-1">
                  <button
                    type="button"
                    className="icon-btn h-8 w-8"
                    disabled={busyId === item.id || item.quantity <= 1}
                    onClick={() => void changeQuantity(item.id, item.quantity - 1, item.availableStock)}
                    aria-label="Decrease quantity"
                  >
                    <Minus className="h-4 w-4" />
                  </button>
                  <span className="w-8 text-center font-semibold">{item.quantity}</span>
                  <button
                    type="button"
                    className="icon-btn h-8 w-8"
                    disabled={busyId === item.id || item.quantity >= item.availableStock}
                    onClick={() => void changeQuantity(item.id, item.quantity + 1, item.availableStock)}
                    aria-label="Increase quantity"
                  >
                    <Plus className="h-4 w-4" />
                  </button>
                </div>
                <p className="w-24 text-right font-display text-lg text-royal">{formatPrice(item.lineTotal)}</p>
                <button
                  type="button"
                  className="icon-btn"
                  disabled={busyId === item.id}
                  onClick={() => void remove(item.id)}
                  aria-label={`Remove ${item.productName}`}
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>
            </li>
          ))}
        </ul>

        <aside className="h-fit card p-6">
          <h2 className="font-display text-xl text-royal">Summary</h2>
          <div className="mt-4 flex justify-between text-sm">
            <span className="text-navy/70">Subtotal</span>
            <span className="font-semibold">{formatPrice(cart.subtotal)}</span>
          </div>
          <p className="mt-2 text-xs text-navy/50">Shipping, tax and discounts are calculated at checkout.</p>
          <Link to="/checkout" className="btn-primary mt-6 w-full">
            Proceed to checkout
          </Link>
          <Link to="/products" className="btn-ghost mt-3 w-full">
            Continue shopping
          </Link>
        </aside>
      </div>
    </div>
  )
}
