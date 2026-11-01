import { useEffect, useState } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { Heart, Menu, Search, ShoppingBag, User, X } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { useCart } from '@/context/CartContext'
import { catalogApi } from '@/services/api'
import type { Category } from '@/types/api.types'

/**
 * Top navigation.
 *
 * - Guests see Shop / Categories / Brands / Skin Types + Login & Register.
 * - Customers additionally see Wishlist, Cart (with live badge) and Account.
 * - Sellers see a link to the Seller Dashboard, admins to the Admin Dashboard.
 *   These links only render for the matching role, and the backend enforces
 *   the same rule independently (route guards are not the security boundary).
 * - The category dropdown is fed by the REAL GET /api/categories response -
 *   there is no hardcoded category list anywhere.
 */
export function Navbar() {
  const { user, isAuthenticated, logout } = useAuth()
  const { itemCount } = useCart()
  const navigate = useNavigate()

  const [categories, setCategories] = useState<Category[]>([])
  const [mobileOpen, setMobileOpen] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')

  const isCustomer = user?.roles.includes('Customer') ?? false
  const isSeller = user?.roles.includes('Seller') ?? false
  const isAdmin = user?.roles.includes('Admin') ?? false

  useEffect(() => {
    // Best-effort load for the dropdown; the navbar must not break if it fails.
    catalogApi
      .getCategories()
      .then(setCategories)
      .catch(() => setCategories([]))
  }, [])

  function submitSearch(event: React.FormEvent) {
    event.preventDefault()
    const value = searchTerm.trim()
    navigate(value ? `/products?search=${encodeURIComponent(value)}` : '/products')
    setMobileOpen(false)
  }

  function handleLogout() {
    logout()
    navigate('/')
  }

  const navLinks = (
    <>
      <NavLink to="/" className="nav-link" onClick={() => setMobileOpen(false)}>
        Home
      </NavLink>
      <NavLink to="/products" className="nav-link" onClick={() => setMobileOpen(false)}>
        Shop
      </NavLink>
      <NavLink to="/products" className="nav-link" onClick={() => setMobileOpen(false)}>
        Categories
      </NavLink>
      <NavLink to="/products" className="nav-link" onClick={() => setMobileOpen(false)}>
        Brands
      </NavLink>
      <NavLink to="/products" className="nav-link" onClick={() => setMobileOpen(false)}>
        Skin Types
      </NavLink>
    </>
  )

  return (
    <header className="sticky top-0 z-40 border-b border-navy/10 bg-off-white/95 backdrop-blur">
      <div className="shell flex h-16 items-center justify-between gap-4">
        {/* Brand */}
        <Link to="/" className="font-display text-2xl font-bold tracking-tight" aria-label="GlowCare home">
          <span className="text-royal">Glow</span><span className="text-yellow">Care</span>
        </Link>

        {/* Desktop nav */}
        <nav className="hidden items-center gap-6 lg:flex" aria-label="Primary">
          {navLinks}
        </nav>

        {/* Search - desktop */}
        <form onSubmit={submitSearch} className="hidden flex-1 justify-end md:flex" role="search">
          <div className="relative w-56">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-navy/40" />
            <input
              type="search"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search products"
              aria-label="Search products"
              className="field pl-9 py-2"
            />
          </div>
        </form>

        {/* Actions */}
        <div className="flex items-center gap-3">
          {isCustomer && (
            <>
              <Link to="/wishlist" className="icon-btn" aria-label="Wishlist">
                <Heart className="h-5 w-5" />
              </Link>
              <Link to="/cart" className="icon-btn relative" aria-label="Cart">
                <ShoppingBag className="h-5 w-5" />
                {itemCount > 0 && (
                  <span className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-yellow px-1 text-[10px] font-bold text-royal">
                    {itemCount}
                  </span>
                )}
              </Link>
            </>
          )}

          {isSeller && (
            <Link to="/seller" className="hidden text-sm font-semibold text-royal sm:block">
              Seller Dashboard
            </Link>
          )}
          {isAdmin && (
            <Link to="/admin" className="hidden text-sm font-semibold text-royal sm:block">
              Admin Dashboard
            </Link>
          )}

          {isAuthenticated ? (
            <div className="hidden items-center gap-3 sm:flex">
              <Link to="/profile" className="icon-btn" aria-label="My account">
                <User className="h-5 w-5" />
              </Link>
              <button type="button" onClick={handleLogout} className="text-sm font-medium text-navy/70 hover:text-royal">
                Logout
              </button>
            </div>
          ) : (
            <div className="hidden items-center gap-2 sm:flex">
              <Link to="/login" className="text-sm font-medium text-navy/70 hover:text-navy">
                Login
              </Link>
              <Link to="/register" className="btn-accent px-4 py-2 text-sm">
                Register
              </Link>
            </div>
          )}

          {/* Mobile toggle */}
          <button
            type="button"
            className="icon-btn lg:hidden"
            aria-label={mobileOpen ? 'Close menu' : 'Open menu'}
            aria-expanded={mobileOpen}
            onClick={() => setMobileOpen((open) => !open)}
          >
            {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
        </div>
      </div>

      {/* Mobile menu */}
      {mobileOpen && (
        <div className="border-t border-navy/10 bg-off-white lg:hidden">
          <div className="shell flex flex-col gap-3 py-4">
            <form onSubmit={submitSearch} role="search">
              <input
                type="search"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search products"
                aria-label="Search products"
                className="field"
              />
            </form>

            <nav className="flex flex-col gap-2" aria-label="Mobile">
              {navLinks}
            </nav>

            {/* Categories from the real API */}
            {categories.length > 0 && (
              <div className="flex flex-col gap-1 border-t border-navy/10 pt-3">
                <span className="text-xs font-semibold uppercase text-navy/50">Shop by category</span>
                {categories.map((category) => (
                  <Link
                    key={category.id}
                    to={`/products?categoryId=${category.id}`}
                    onClick={() => setMobileOpen(false)}
                    className="text-sm text-navy/80"
                  >
                    {category.name}
                  </Link>
                ))}
              </div>
            )}

            <div className="flex flex-col gap-2 border-t border-navy/10 pt-3">
              {isAuthenticated ? (
                <>
                  <Link to="/profile" onClick={() => setMobileOpen(false)}>My Account</Link>
                  {isCustomer && <Link to="/orders" onClick={() => setMobileOpen(false)}>My Orders</Link>}
                  {isSeller && <Link to="/seller" onClick={() => setMobileOpen(false)}>Seller Dashboard</Link>}
                  {isAdmin && <Link to="/admin" onClick={() => setMobileOpen(false)}>Admin Dashboard</Link>}
                  <button type="button" onClick={handleLogout} className="text-left font-medium text-royal">
                    Logout
                  </button>
                </>
              ) : (
                <>
                  <Link to="/login" onClick={() => setMobileOpen(false)}>Login</Link>
                  <Link to="/register" onClick={() => setMobileOpen(false)}>Register</Link>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </header>
  )
}
