import { Route, Routes } from 'react-router-dom'
import { Navbar } from '@/components/layout/Navbar'
import { Footer } from '@/components/layout/Footer'
import { GuestOnlyRoute, ProtectedRoute, RoleRoute } from '@/routes/guards'
import { HomePage } from '@/pages/Home/HomePage'
import { ProductsPage } from '@/pages/Products/ProductsPage'
import { ProductDetailsPage } from '@/pages/ProductDetails/ProductDetailsPage'
import { LoginPage } from '@/pages/Auth/LoginPage'
import { RegisterPage } from '@/pages/Auth/RegisterPage'
import { CartPage } from '@/pages/Cart/CartPage'
import { CheckoutPage } from '@/pages/Checkout/CheckoutPage'
import { OrdersPage } from '@/pages/Orders/OrdersPage'
import { OrderDetailsPage } from '@/pages/Orders/OrderDetailsPage'
import { WishlistPage } from '@/pages/Wishlist/WishlistPage'
import { ProfilePage } from '@/pages/Account/ProfilePage'
import { AboutPage } from '@/pages/About/AboutPage'
import { SellerDashboardPage } from '@/pages/Seller/SellerDashboardPage'
import { SellerProductsPage } from '@/pages/Seller/SellerProductsPage'
import { SellerOrdersPage } from '@/pages/Seller/SellerOrdersPage'
import { AdminDashboardPage } from '@/pages/Admin/AdminDashboardPage'
import { AdminUsersPage } from '@/pages/Admin/AdminUsersPage'
import { AdminSellersPage } from '@/pages/Admin/AdminSellersPage'
import { AdminCategoriesPage } from '@/pages/Admin/AdminCategoriesPage'
import { AdminBrandsPage } from '@/pages/Admin/AdminBrandsPage'
import { AdminSkinTypesPage } from '@/pages/Admin/AdminSkinTypesPage'
import { AdminProductsPage } from '@/pages/Admin/AdminProductsPage'
import { AdminOrdersPage } from '@/pages/Admin/AdminOrdersPage'
import { AdminPromosPage } from '@/pages/Admin/AdminPromosPage'
import { AdminBannersPage } from '@/pages/Admin/AdminBannersPage'
import { AdminPaymentsPage } from '@/pages/Admin/AdminPaymentsPage'
import { AdminReviewsPage } from '@/pages/Admin/AdminReviewsPage'
import { PaymentReturnPage } from '@/pages/PaymentReturnPage'
import { NotFoundPage } from '@/pages/NotFound/NotFoundPage'

/**
 * Application shell + route table.
 *
 * Phase 10 covered the public storefront (home, catalog) + authentication.
 * Phase 11 adds product details. The guarded customer/seller/admin areas are
 * added in the following phases and slot into the same <Routes> block.
 */
export default function App() {
  return (
    <div className="flex min-h-screen flex-col">
      <Navbar />
      <main className="flex-1">
        <Routes>
          {/* Public */}
          <Route path="/" element={<HomePage />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/products/:id" element={<ProductDetailsPage />} />
          <Route path="/about" element={<AboutPage />} />

          {/* Guests only - signed-in users are bounced home. */}
          <Route
            path="/login"
            element={
              <GuestOnlyRoute>
                <LoginPage />
              </GuestOnlyRoute>
            }
          />
          <Route
            path="/register"
            element={
              <GuestOnlyRoute>
                <RegisterPage />
              </GuestOnlyRoute>
            }
          />

          {/* Cart - accessible to guests, checkout requires auth */}
          <Route path="/cart" element={<CartPage />} />
          <Route
            path="/checkout"
            element={
              <ProtectedRoute>
                <CheckoutPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/payment/return"
            element={
              <ProtectedRoute>
                <PaymentReturnPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/orders"
            element={
              <ProtectedRoute>
                <OrdersPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/orders/:id"
            element={
              <ProtectedRoute>
                <OrderDetailsPage />
              </ProtectedRoute>
            }
          />
          <Route path="/wishlist" element={<WishlistPage />} />
          <Route
            path="/account"
            element={
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            }
          />

          {/* Seller routes */}
          <Route
            path="/seller"
            element={
              <RoleRoute roles={['Seller']}>
                <SellerDashboardPage />
              </RoleRoute>
            }
          />
          <Route
            path="/seller/products"
            element={
              <RoleRoute roles={['Seller']}>
                <SellerProductsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/seller/orders"
            element={
              <RoleRoute roles={['Seller']}>
                <SellerOrdersPage />
              </RoleRoute>
            }
          />

          {/* Admin routes */}
          <Route
            path="/admin"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminDashboardPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/users"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminUsersPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/sellers"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminSellersPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/categories"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminCategoriesPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/brands"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminBrandsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/skintypes"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminSkinTypesPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/products"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminProductsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/orders"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminOrdersPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/promos"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminPromosPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/banners"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminBannersPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/payments"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminPaymentsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/admin/reviews"
            element={
              <RoleRoute roles={['Admin']}>
                <AdminReviewsPage />
              </RoleRoute>
            }
          />

          {/* Catch-all */}
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
      <Footer />
    </div>
  )
}
