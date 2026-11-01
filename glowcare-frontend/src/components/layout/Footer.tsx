import { Link } from 'react-router-dom'
import { Container } from './Container'

const CURRENT_YEAR = new Date().getFullYear()

/**
 * Site footer - Primary Blue surface with yellow accents and white text,
 * matching the reference palette. Links are static navigation only (no data
 * fetch needed); every target is a route that exists in App.tsx.
 */
export function Footer() {
  return (
    <footer className="mt-20 bg-royal text-white">
      <Container className="grid gap-10 py-14 sm:grid-cols-2 lg:grid-cols-4">
        <div>
          <p className="font-display text-2xl">
            GLOW<span className="text-yellow">CARE</span>
          </p>
          <p className="mt-3 text-sm text-white/80">
            A skincare marketplace connecting you with trusted brands and sellers,
            curated for every skin type.
          </p>
        </div>

        <div>
          <h4 className="font-display text-sm uppercase tracking-widest text-yellow">Shop</h4>
          <ul className="mt-3 space-y-2 text-sm text-white/80">
            <li><Link to="/products" className="hover:text-yellow">All Products</Link></li>
            <li><Link to="/products" className="hover:text-yellow">Categories</Link></li>
            <li><Link to="/products" className="hover:text-yellow">Brands</Link></li>
            <li><Link to="/products" className="hover:text-yellow">By Skin Type</Link></li>
          </ul>
        </div>

        <div>
          <h4 className="font-display text-sm uppercase tracking-widest text-yellow">Customer Care</h4>
          <ul className="mt-3 space-y-2 text-sm text-white/80">
            <li><Link to="/orders" className="hover:text-yellow">Track Your Order</Link></li>
            <li><Link to="/cart" className="hover:text-yellow">Your Cart</Link></li>
            <li><Link to="/wishlist" className="hover:text-yellow">Wishlist</Link></li>
            <li><Link to="/profile" className="hover:text-yellow">Account</Link></li>
          </ul>
        </div>

        <div>
          <h4 className="font-display text-sm uppercase tracking-widest text-yellow">About</h4>
          <ul className="mt-3 space-y-2 text-sm text-white/80">
            <li><Link to="/" className="hover:text-yellow">Our Philosophy</Link></li>
            <li><Link to="/seller" className="hover:text-yellow">Sell on GlowCare</Link></li>
          </ul>
          <p className="mt-4 text-sm text-white/70">
            Cairo, Egypt
            <br />
            support@glowcare.com
          </p>
        </div>
      </Container>

      <div className="border-t border-white/20 py-5">
        <Container className="flex flex-col items-center justify-between gap-2 text-xs text-white/70 sm:flex-row">
          <p>&copy; {CURRENT_YEAR} GlowCare. All rights reserved.</p>
          <p>Built with ASP.NET Core &amp; React</p>
        </Container>
      </div>
    </footer>
  )
}
