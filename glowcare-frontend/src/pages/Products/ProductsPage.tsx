import { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { SlidersHorizontal, X } from 'lucide-react'
import { ProductCard } from '@/components/products/ProductCard'
import { Pagination } from '@/components/common/Pagination'
import { ErrorState, Spinner } from '@/components/common/States'
import { catalogApi } from '@/services/api'
import type { Brand, Category, ProductFilter, ProductListItem, SkinType } from '@/types/api.types'
import { getErrorMessage } from '@/utils/errors'
import { sortOptions } from '@/utils/labels'

const PAGE_SIZE = 12

/**
 * Catalog page: search + filters + sort + pagination, all driven by the real
 * GET /api/products endpoint (the backend does the filtering and paging).
 *
 * The URL is the source of truth for the query (?search=&categoryId=...), so a
 * filtered view is shareable and the back button behaves as users expect.
 */
export function ProductsPage() {
  const [searchParams, setSearchParams] = useSearchParams()

  const [products, setProducts] = useState<ProductListItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [mobileFilterOpen, setMobileFilterOpen] = useState(false)

  // Filter options themselves come from the API (never hardcoded).
  const [categories, setCategories] = useState<Category[]>([])
  const [brands, setBrands] = useState<Brand[]>([])
  const [skinTypes, setSkinTypes] = useState<SkinType[]>([])

  // Local draft state for the price inputs, applied on submit.
  const [minPriceInput, setMinPriceInput] = useState(searchParams.get('minPrice') ?? '')
  const [maxPriceInput, setMaxPriceInput] = useState(searchParams.get('maxPrice') ?? '')

  const filter: ProductFilter = useMemo(() => {
    const result: ProductFilter = { page: Number(searchParams.get('page') ?? '1'), pageSize: PAGE_SIZE }
    const search = searchParams.get('search')
    const categoryId = searchParams.get('categoryId')
    const brandId = searchParams.get('brandId')
    const skinTypeId = searchParams.get('skinTypeId')
    const minPrice = searchParams.get('minPrice')
    const maxPrice = searchParams.get('maxPrice')
    const sortBy = searchParams.get('sortBy')

    if (search) result.search = search
    if (categoryId) result.categoryId = Number(categoryId)
    if (brandId) result.brandId = Number(brandId)
    if (skinTypeId) result.skinTypeId = Number(skinTypeId)
    if (minPrice) result.minPrice = Number(minPrice)
    if (maxPrice) result.maxPrice = Number(maxPrice)
    if (sortBy) result.sortBy = sortBy
    return result
  }, [searchParams])

  // Load the filter dropdowns once.
  useEffect(() => {
    let cancelled = false
    Promise.allSettled([
      catalogApi.getCategories(),
      catalogApi.getBrands(),
      catalogApi.getSkinTypes(),
    ]).then(([categoryResult, brandResult, skinTypeResult]) => {
      if (cancelled) return
      if (categoryResult.status === 'fulfilled') setCategories(categoryResult.value)
      if (brandResult.status === 'fulfilled') setBrands(brandResult.value)
      if (skinTypeResult.status === 'fulfilled') setSkinTypes(skinTypeResult.value)
    })
    return () => {
      cancelled = true
    }
  }, [])

  // Fetch the product page whenever the URL query changes.
  const fetchProducts = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await catalogApi.getProducts(filter)
      setProducts(result.items)
      setTotalCount(result.totalCount)
      setTotalPages(result.totalPages)
    } catch (err) {
      setError(getErrorMessage(err, 'Could not load products.'))
    } finally {
      setLoading(false)
    }
  }, [filter])

  useEffect(() => {
    void fetchProducts()
  }, [fetchProducts])

  /** Merges a patch into the URL query, resetting to page 1 on filter changes. */
  function applyFilter(patch: Record<string, string | null>, keepPage = false) {
    const next = new URLSearchParams(searchParams)
    for (const [key, value] of Object.entries(patch)) {
      if (value === null || value === '') next.delete(key)
      else next.set(key, value)
    }
    if (!keepPage) next.delete('page')
    setSearchParams(next)
  }

  const activeFilterCount = ['search', 'categoryId', 'brandId', 'skinTypeId', 'minPrice', 'maxPrice'].filter(
    (key) => searchParams.get(key),
  ).length

  return (
    <div className="shell py-10">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="heading-display uppercase">Shop</h1>
          <p className="mt-1 text-navy/60">
            {loading ? 'Loading...' : `${totalCount} product${totalCount === 1 ? '' : 's'} found`}
          </p>
        </div>

        <div className="w-56">
          <label htmlFor="sort" className="sr-only">
            Sort products
          </label>
          <select
            id="sort"
            className="field py-2"
            value={searchParams.get('sortBy') ?? 'newest'}
            onChange={(e) => applyFilter({ sortBy: e.target.value })}
          >
            {sortOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div className="mt-8 grid gap-8 lg:grid-cols-[16rem_1fr]">
        {/* Mobile filter button */}
        <button
          type="button"
          onClick={() => setMobileFilterOpen(true)}
          className="lg:hidden flex items-center gap-2 rounded-xl bg-white px-4 py-2.5 text-sm font-semibold text-royal shadow-sm"
        >
          <SlidersHorizontal className="h-4 w-4" />
          Filters
          {activeFilterCount > 0 && (
            <span className="ml-1 rounded-full bg-yellow px-2 py-0.5 text-xs">{activeFilterCount}</span>
          )}
        </button>

        {/* ---------------------------------------------------------- */}
        {/* Filters sidebar (desktop) / drawer (mobile)               */}
        {/* ---------------------------------------------------------- */}
        <aside className={`h-fit card p-5 lg:block ${mobileFilterOpen ? 'fixed inset-0 z-50 m-0 h-full overflow-y-auto lg:relative lg:z-auto lg:h-fit lg:overflow-visible' : 'hidden lg:block'}`}>
          <div className="flex items-center justify-between">
            <h2 className="flex items-center gap-2 font-display text-lg text-navy">
              <SlidersHorizontal className="h-4 w-4" /> Filters
            </h2>
            <div className="flex items-center gap-2">
              {activeFilterCount > 0 && (
                <button
                  type="button"
                  onClick={() => {
                    setSearchParams({})
                    setMobileFilterOpen(false)
                  }}
                  className="text-sm font-medium text-royal hover:underline"
                >
                  Clear all
                </button>
              )}
              <button
                type="button"
                onClick={() => setMobileFilterOpen(false)}
                className="lg:hidden icon-btn"
                aria-label="Close filters"
              >
                <X className="h-5 w-5" />
              </button>
            </div>
          </div>

          {/* Search */}
          <form
            className="mt-4"
            onSubmit={(e) => {
              e.preventDefault()
              const value = new FormData(e.currentTarget).get('search') as string
              applyFilter({ search: value?.trim() || null })
            }}
          >
            <label htmlFor="filter-search" className="field-label">
              Search
            </label>
            <input
              id="filter-search"
              name="search"
              defaultValue={searchParams.get('search') ?? ''}
              placeholder="e.g. serum"
              className="field py-2"
            />
          </form>

          {/* Category */}
          <div className="mt-4">
            <label htmlFor="filter-category" className="field-label">
              Category
            </label>
            <select
              id="filter-category"
              className="field py-2"
              value={searchParams.get('categoryId') ?? ''}
              onChange={(e) => applyFilter({ categoryId: e.target.value || null })}
            >
              <option value="">All categories</option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </div>

          {/* Brand */}
          <div className="mt-4">
            <label htmlFor="filter-brand" className="field-label">
              Brand
            </label>
            <select
              id="filter-brand"
              className="field py-2"
              value={searchParams.get('brandId') ?? ''}
              onChange={(e) => applyFilter({ brandId: e.target.value || null })}
            >
              <option value="">All brands</option>
              {brands.map((brand) => (
                <option key={brand.id} value={brand.id}>
                  {brand.name}
                </option>
              ))}
            </select>
          </div>

          {/* Skin type */}
          <div className="mt-4">
            <label htmlFor="filter-skin-type" className="field-label">
              Skin type
            </label>
            <select
              id="filter-skin-type"
              className="field py-2"
              value={searchParams.get('skinTypeId') ?? ''}
              onChange={(e) => applyFilter({ skinTypeId: e.target.value || null })}
            >
              <option value="">All skin types</option>
              {skinTypes.map((skinType) => (
                <option key={skinType.id} value={skinType.id}>
                  {skinType.name}
                </option>
              ))}
            </select>
          </div>

          {/* Price range */}
          <form
            className="mt-4"
            onSubmit={(e) => {
              e.preventDefault()
              applyFilter({
                minPrice: minPriceInput.trim() || null,
                maxPrice: maxPriceInput.trim() || null,
              })
              setMobileFilterOpen(false)
            }}
          >
            <span className="field-label">Price (EGP)</span>
            <div className="flex items-center gap-2">
              <input
                aria-label="Minimum price"
                inputMode="decimal"
                value={minPriceInput}
                onChange={(e) => setMinPriceInput(e.target.value)}
                placeholder="Min"
                className="field py-2"
              />
              <span className="text-navy/40">-</span>
              <input
                aria-label="Maximum price"
                inputMode="decimal"
                value={maxPriceInput}
                onChange={(e) => setMaxPriceInput(e.target.value)}
                placeholder="Max"
                className="field py-2"
              />
            </div>
            <button type="submit" className="btn-ghost mt-3 w-full py-2 text-sm">
              Apply price
            </button>
          </form>
        </aside>

        {/* Mobile filter overlay */}
        {mobileFilterOpen && (
          <div
            className="fixed inset-0 z-40 bg-black/50 lg:hidden"
            onClick={() => setMobileFilterOpen(false)}
          />
        )}

        {/* ---------------------------------------------------------- */}
        {/* Results grid                                                 */}
        {/* ---------------------------------------------------------- */}
        <section>
          {loading ? (
            <Spinner label="Loading products..." />
          ) : error ? (
            <ErrorState message={error} onRetry={() => void fetchProducts()} />
          ) : products.length === 0 ? (
            <div className="card px-6 py-16 text-center">
              <h3 className="heading-panel">No products match</h3>
              <p className="mt-2 text-navy/60">Try widening your filters or search term.</p>
              <button type="button" onClick={() => setSearchParams({})} className="btn-primary mt-4">
                Clear filters
              </button>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-2 gap-5 md:grid-cols-3">
                {products.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </div>
              <Pagination
                page={filter.page ?? 1}
                totalPages={totalPages}
                onChange={(page) => applyFilter({ page: String(page) }, true)}
              />
            </>
          )}
        </section>
      </div>
    </div>
  )
}
