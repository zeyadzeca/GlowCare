import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { DashboardShell } from '@/components/dashboard/DashboardShell'
import { Spinner } from '@/components/common/States'
import { Button } from '@/components/common/Button'
import { Input } from '@/components/common/FormField'
import { sellerApi, catalogApi } from '@/services/api'
import { getErrorMessage } from '@/utils/errors'
import { formatPrice } from '@/utils/format'
import type { ProductDetails, ProductSaveRequest } from '@/types/api.types'

export function SellerProductsPage() {
  const [products, setProducts] = useState<ProductDetails[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterCategory, setFilterCategory] = useState<number | 0>(0)
  const [filterBrand, setFilterBrand] = useState<number | 0>(0)
  const [filterActive, setFilterActive] = useState<boolean | null>(null)
  const [form, setForm] = useState<ProductSaveRequest>({
    name: '',
    description: '',
    price: 0,
    stockQuantity: 0,
    categoryId: 0,
    brandId: 0,
    volume: '',
    spf: null,
    imageUrls: [],
    skinTypeIds: [],
    ingredientIds: [],
  })
  const [categories, setCategories] = useState<any[]>([])
  const [brands, setBrands] = useState<any[]>([])
  const [skinTypes, setSkinTypes] = useState<any[]>([])
  const [ingredients, setIngredients] = useState<any[]>([])

  const load = () => {
    setLoading(true)
    setError(null)
    Promise.all([
      sellerApi.getMyProducts().then(setProducts),
      catalogApi.getCategories().then(setCategories),
      catalogApi.getBrands().then(setBrands),
      catalogApi.getSkinTypes().then(setSkinTypes),
      catalogApi.getIngredients().then((ings) => setIngredients(ings.filter((v, i, a) => a.findIndex((x) => x.id === v.id) === i))),
    ])
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [])

  const filteredProducts = products?.filter((p) => {
    if (searchTerm && !p.name.toLowerCase().includes(searchTerm.toLowerCase())) return false
    if (filterCategory && p.categoryId !== filterCategory) return false
    if (filterBrand && p.brandId !== filterBrand) return false
    if (filterActive !== null && p.isActive !== filterActive) return false
    return true
  })

  async function handleToggleActive(id: number, isActive: boolean) {
    setBusyId(id)
    setError(null)
    try {
      if (isActive) {
        await sellerApi.deactivateProduct(id)
      } else {
        await sellerApi.activateProduct(id)
      }
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }

  function openForm(product?: ProductDetails) {
    if (product) {
      setEditingId(product.id)
      setForm({
        name: product.name,
        description: product.description,
        price: product.price,
        stockQuantity: product.stockQuantity,
        categoryId: product.categoryId,
        brandId: product.brandId,
        volume: product.volume ?? '',
        spf: product.spf,
        imageUrls: product.images,
        skinTypeIds: product.skinTypes.map((s: any) => s.id),
        ingredientIds: product.ingredients.map((i: any) => i.id),
      })
    } else {
      setEditingId(null)
      setForm({
        name: '',
        description: '',
        price: 0,
        stockQuantity: 0,
        categoryId: 0,
        brandId: 0,
        volume: '',
        spf: null,
        imageUrls: [],
        skinTypeIds: [],
        ingredientIds: [],
      })
    }
    setShowForm(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setBusyId(editingId ?? -1)
    try {
      if (editingId) {
        await sellerApi.updateProduct(editingId, form)
      } else {
        await sellerApi.createProduct(form)
      }
      setShowForm(false)
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusyId(null)
    }
  }



  return (
    <DashboardShell
      title="My Products"
      subtitle="Manage your product catalog"
      nav={[
        { to: '/seller', label: 'Overview' },
        { to: '/seller/products', label: 'Products', end: true },
        { to: '/seller/orders', label: 'Orders' },
      ]}
    >
      {error && <p className="text-sm text-red-600">{error}</p>}
      {showForm ? (
        <div className="card p-6">
          <h3 className="font-display text-lg text-royal mb-4">{editingId ? 'Edit Product' : 'Add Product'}</h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            <Input label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} required />
            <div className="grid gap-4 sm:grid-cols-2">
              <Input label="Price" type="number" value={form.price} onChange={(e) => setForm({ ...form, price: Number(e.target.value) })} required />
              <Input label="Stock" type="number" value={form.stockQuantity} onChange={(e) => setForm({ ...form, stockQuantity: Number(e.target.value) })} required />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <select className="field" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })} required>
                <option value="">Select category</option>
                {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
              <select className="field" value={form.brandId} onChange={(e) => setForm({ ...form, brandId: Number(e.target.value) })} required>
                <option value="">Select brand</option>
                {brands.map((b) => <option key={b.id} value={b.id}>{b.name}</option>)}
              </select>
            </div>
            <Input label="Volume" value={form.volume} onChange={(e) => setForm({ ...form, volume: e.target.value })} />
            <Input label="SPF" type="number" value={form.spf ?? ''} onChange={(e) => setForm({ ...form, spf: e.target.value ? Number(e.target.value) : null })} placeholder="Optional" />
            <Input label="Image URLs (comma-separated)" value={form.imageUrls.join(', ')} onChange={(e) => setForm({ ...form, imageUrls: e.target.value.split(',').map((s) => s.trim()).filter(Boolean) })} placeholder="https://..." />
            <div>
              <label className="block text-sm font-medium text-navy mb-2">Skin Types</label>
              <div className="grid grid-cols-2 gap-2">
                {skinTypes.map((st) => (
                  <label key={st.id} className="flex items-center gap-2 text-sm">
                    <input type="checkbox" checked={form.skinTypeIds.includes(st.id)} onChange={(e) => {
                      if (e.target.checked) {
                        setForm({ ...form, skinTypeIds: [...form.skinTypeIds, st.id] })
                      } else {
                        setForm({ ...form, skinTypeIds: form.skinTypeIds.filter((id) => id !== st.id) })
                      }
                    }} />
                    {st.name}
                  </label>
                ))}
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-navy mb-2">Ingredients</label>
              <div className="grid grid-cols-2 gap-2 max-h-32 overflow-y-auto">
                {ingredients.map((ing) => (
                  <label key={ing.id} className="flex items-center gap-2 text-sm">
                    <input type="checkbox" checked={form.ingredientIds.includes(ing.id)} onChange={(e) => {
                      if (e.target.checked) {
                        setForm({ ...form, ingredientIds: [...form.ingredientIds, ing.id] })
                      } else {
                        setForm({ ...form, ingredientIds: form.ingredientIds.filter((id) => id !== ing.id) })
                      }
                    }} />
                    {ing.name}
                  </label>
                ))}
              </div>
            </div>
            <div className="flex gap-2">
              <Button type="submit" loading={busyId === -1}>{editingId ? 'Update' : 'Create'}</Button>
              <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
            </div>
          </form>
        </div>
      ) : loading ? (
        <div className="card p-8 text-center">
          <Spinner label="Loading products..." />
        </div>
      ) : (
        <>
          <div className="mb-4 flex flex-wrap gap-4 items-center">
            <Button onClick={() => openForm()}>Add Product</Button>
            <input type="text" placeholder="Search products..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} className="field flex-1 min-w-[200px]" />
            <select className="field" value={filterCategory} onChange={(e) => setFilterCategory(Number(e.target.value))}>
              <option value={0}>All Categories</option>
              {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            <select className="field" value={filterBrand} onChange={(e) => setFilterBrand(Number(e.target.value))}>
              <option value={0}>All Brands</option>
              {brands.map((b) => <option key={b.id} value={b.id}>{b.name}</option>)}
            </select>
            <select className="field" value={filterActive === null ? '' : filterActive ? 'true' : 'false'} onChange={(e) => setFilterActive(e.target.value === '' ? null : e.target.value === 'true')}>
              <option value="">All Status</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </div>
          {filteredProducts && filteredProducts.length > 0 ? (
            <div className="card overflow-hidden">
              <table className="w-full">
                <thead className="bg-navy/5">
                  <tr>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Product</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Price</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Stock</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Status</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-navy">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredProducts.map((product) => (
                    <tr key={product.id} className="border-t border-navy/10">
                      <td className="px-4 py-3">
                        <Link to={`/products/${product.id}`} className="font-display text-sm text-royal hover:underline">
                          {product.name}
                        </Link>
                        <p className="text-xs text-navy/50">{product.brandName}</p>
                      </td>
                      <td className="px-4 py-3 text-sm text-navy">{formatPrice(product.price)}</td>
                      <td className="px-4 py-3 text-sm text-navy">
                        <span className={product.stockQuantity <= 5 ? 'text-red-600 font-semibold' : product.stockQuantity === 0 ? 'text-orange-600 font-semibold' : 'text-navy'}>
                          {product.stockQuantity} {product.stockQuantity === 0 ? '(Out of stock)' : product.stockQuantity <= 5 ? '(Low stock)' : ''}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-sm">
                        {product.isActive ? (
                          <span className="text-green-600">Active</span>
                        ) : (
                          <span className="text-red-600">Inactive</span>
                        )}
                      </td>
                      <td className="px-4 py-3 flex gap-2">
                        <Button variant="ghost" className="text-sm" onClick={() => openForm(product)}>Edit</Button>
                        <Button variant="ghost" className="text-sm" loading={busyId === product.id} onClick={() => void handleToggleActive(product.id, product.isActive)}>
                          {product.isActive ? 'Deactivate' : 'Activate'}
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-navy/60">No products found matching your filters.</p>
          )}
        </>
      )}
    </DashboardShell>
  )
}
