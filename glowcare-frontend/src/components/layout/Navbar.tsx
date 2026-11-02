import { useEffect, useState } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { Heart, Menu, Search, ShoppingBag, User, X, ChevronDown, Package } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { useCart } from '@/context/CartContext'
import { catalogApi } from '@/services/api'
import type { Brand, Category, SkinType } from '@/types/api.types'

/**
 * Top navigation.
 *
 * - Guests see Shop / Categories / Brands / Skin Types + Login & Register.
 * - Customers additionally see Wishlist, Cart (with live badge) and Account.
 * - Sellers see a link to the Seller Dashboard, admins to the Admin Dashboard.
 *   These links only render for the matching role, and the backend enforces
 *   the same rule independently (route guards are not the security boundary).
 * - Dropdowns are fed by REAL API responses - no hardcoded lists.
 */
export function Navbar() {
  const { user, isAuthenticated, logout } = useAuth()
  const { itemCount } = useCart()
  const navigate = useNavigate()

  const [categories, setCategories] = useState<Category[]>([])
  const [brands, setBrands] = useState<Brand[]>([])
  const [skinTypes, setSkinTypes] = useState<SkinType[]>([])
  const [mobileOpen, setMobileOpen] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [openDropdown, setOpenDropdown] = useState<string | null>(null)

  const isCustomer = user?.roles.includes('Customer') ?? false
  const isSeller = user?.roles.includes('Seller') ?? false
  const isAdmin = user?.roles.includes('Admin') ?? false

  useEffect(() => {
    Promise.all([
      catalogApi.getCategories().then(setCategories).catch(() => setCategories([])),
      catalogApi.getBrands().then(setBrands).catch(() => setBrands([])),
      catalogApi.getSkinTypes().then(setSkinTypes).catch(() => setSkinTypes([])),
    ])
  }, [])

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (openDropdown && !(e.target as HTMLElement).closest('.dropdown-trigger')) {
        setOpenDropdown(null)
      }
    }
    document.addEventListener('click', handleClickOutside)
    return () => document.removeEventListener('click', handleClickOutside)
  }, [openDropdown])

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
      <div className="relative dropdown-trigger">
        <button
          type="button"
          className="nav-link flex items-center gap-1"
          onClick={() => setOpenDropdown(openDropdown === 'categories' ? null : 'categories')}
        >
          Categories <ChevronDown className="h-4 w-4" />
        </button>
        {openDropdown === 'categories' && (
          <div className="absolute left-0 top-full z-50 mt-2 max-h-64 w-48 overflow-y-auto overscroll-contain rounded-lg border border-navy/10 bg-white p-2 shadow-lg">
            {categories.length > 0 ? (
              categories.map((category) => (
                <Link
                  key={category.id}
                  to={`/products?categoryId=${category.id}`}
                  className="block rounded px-3 py-2 text-sm text-navy hover:bg-navy/5"
                  onClick={() => setOpenDropdown(null)}
                >
                  {category.name}
                </Link>
              ))
            ) : (
              <p className="px-3 py-2 text-sm text-navy/50">No categories</p>
            )}
          </div>
        )}
      </div>
      <div className="relative dropdown-trigger">
        <button
          type="button"
          className="nav-link flex items-center gap-1"
          onClick={() => setOpenDropdown(openDropdown === 'brands' ? null : 'brands')}
        >
          Brands <ChevronDown className="h-4 w-4" />
        </button>
        {openDropdown === 'brands' && (
          <div className="absolute left-0 top-full z-50 mt-2 max-h-64 w-48 overflow-y-auto overscroll-contain rounded-lg border border-navy/10 bg-white p-2 shadow-lg">
            {brands.length > 0 ? (
              brands.map((brand) => (
                <Link
                  key={brand.id}
                  to={`/products?brandId=${brand.id}`}
                  className="block rounded px-3 py-2 text-sm text-navy hover:bg-navy/5"
                  onClick={() => setOpenDropdown(null)}
                >
                  {brand.name}
                </Link>
              ))
            ) : (
              <p className="px-3 py-2 text-sm text-navy/50">No brands</p>
            )}
          </div>
        )}
      </div>
      <div className="relative dropdown-trigger">
        <button
          type="button"
          className="nav-link flex items-center gap-1"
          onClick={() => setOpenDropdown(openDropdown === 'skinTypes' ? null : 'skinTypes')}
        >
          Skin Types <ChevronDown className="h-4 w-4" />
        </button>
        {openDropdown === 'skinTypes' && (
          <div className="absolute left-0 top-full z-50 mt-2 max-h-64 w-48 overflow-y-auto overscroll-contain rounded-lg border border-navy/10 bg-white p-2 shadow-lg">
            {skinTypes.length > 0 ? (
              skinTypes.map((skinType) => (
                <Link
                  key={skinType.id}
                  to={`/products?skinTypeId=${skinType.id}`}
                  className="block rounded px-3 py-2 text-sm text-navy hover:bg-navy/5"
                  onClick={() => setOpenDropdown(null)}
                >
                  {skinType.name}
                </Link>
              ))
            ) : (
              <p className="px-3 py-2 text-sm text-navy/50">No skin types</p>
            )}
          </div>
        )}
      </div>
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
          {(isCustomer || !isAuthenticated) && (
            <Link to="/wishlist" className="icon-btn" aria-label="Wishlist">
              <Heart className="h-5 w-5" />
            </Link>
          )}
          {isCustomer ? (
            <Link to="/orders" className="icon-btn" aria-label="My Orders">
              <Package className="h-5 w-5" />
            </Link>
          ) : !isAuthenticated && (
            <Link to="/login?redirect=/orders" className="icon-btn" aria-label="My Orders">
              <Package className="h-5 w-5" />
            </Link>
          )}
          {(isCustomer || !isAuthenticated) && (
            <Link to="/cart" className="icon-btn relative" aria-label="Cart">
              <ShoppingBag className="h-5 w-5" />
              {itemCount > 0 && (
                <span className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-yellow px-1 text-[10px] font-bold text-royal">
                  {itemCount}
                </span>
              )}
            </Link>
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
              <Link to="/account" className="icon-btn" aria-label="My account">
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

            {brands.length > 0 && (
              <div className="flex flex-col gap-1 border-t border-navy/10 pt-3">
                <span className="text-xs font-semibold uppercase text-navy/50">Shop by brand</span>
                {brands.map((brand) => (
                  <Link
                    key={brand.id}
                    to={`/products?brandId=${brand.id}`}
                    onClick={() => setMobileOpen(false)}
                    className="text-sm text-navy/80"
                  >
                    {brand.name}
                  </Link>
                ))}
              </div>
            )}

            {skinTypes.length > 0 && (
              <div className="flex flex-col gap-1 border-t border-navy/10 pt-3">
                <span className="text-xs font-semibold uppercase text-navy/50">Shop by skin type</span>
                {skinTypes.map((skinType) => (
                  <Link
                    key={skinType.id}
                    to={`/products?skinTypeId=${skinType.id}`}
                    onClick={() => setMobileOpen(false)}
                    className="text-sm text-navy/80"
                  >
                    {skinType.name}
                  </Link>
                ))}
              </div>
            )}

            <div className="flex flex-col gap-2 border-t border-navy/10 pt-3">
              {isAuthenticated ? (
                <>
                  <Link to="/account" onClick={() => setMobileOpen(false)}>My Account</Link>
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
