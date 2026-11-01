import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ArrowRight, Check } from 'lucide-react'
import { Container } from '@/components/layout/Container'
import { ProductCard } from '@/components/products/ProductCard'
import { Spinner } from '@/components/common/States'
import { EditorialImage } from '@/components/common/EditorialImage'
import { CircularBadge } from '@/components/common/CircularBadge'
import { catalogApi, promoBannerApi } from '@/services/api'
import type { Banner, Brand, Category, ProductListItem, SkinType } from '@/types/api.types'

/**
 * Home page.
 *
 * Layout + colour balance follow the two approved reference images:
 *   - Bright YELLOW hero with an oversized condensed BLUE headline and an
 *     organic-shaped image (reference 1).
 *   - Warm OFF-WHITE editorial split with a SKY-BLUE elliptical portrait and
 *     the oversized blue headline (reference 2).
 *
 * Every section is rendered from REAL API responses - no mock data or
 * hardcoded product/brand/category list anywhere. Sections degrade gracefully:
 * if one request fails, the rest of the page still renders.
 */

/** Value bullets from reference 1 (blue text, yellow check discs). */
const VALUE_PROPS = [
  '100% transparent formulas',
  'Only verified ingredients',
  'Filter by skin type & actives',
  'Verified seller marketplace',
]

export function HomePage() {
  const [products, setProducts] = useState<ProductListItem[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [brands, setBrands] = useState<Brand[]>([])
  const [skinTypes, setSkinTypes] = useState<SkinType[]>([])
  const [banners, setBanners] = useState<Banner[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false

    async function load() {
      // Fire all public reads together; a failure in one does not block the rest.
      const [productResult, categoryResult, brandResult, skinTypeResult, bannerResult] =
        await Promise.allSettled([
          catalogApi.getProducts({ pageSize: 8, sortBy: 'newest' }),
          catalogApi.getCategories(),
          catalogApi.getBrands(),
          catalogApi.getSkinTypes(),
          promoBannerApi.getActiveBanners(),
        ])

      if (cancelled) return

      if (productResult.status === 'fulfilled') setProducts(productResult.value.items)
      if (categoryResult.status === 'fulfilled') setCategories(categoryResult.value)
      if (brandResult.status === 'fulfilled') setBrands(brandResult.value)
      if (skinTypeResult.status === 'fulfilled') setSkinTypes(skinTypeResult.value)
      if (bannerResult.status === 'fulfilled') setBanners(bannerResult.value)

      setLoading(false)
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <div>
      {/* ================================================================
          1. HERO - reference 1: bright yellow, oversized blue headline,
             organic image, white pill CTA, value bullets.
      ================================================================ */}
      <section className="overflow-hidden bg-yellow">
        <Container className="grid items-center gap-12 py-14 lg:grid-cols-2 lg:py-20">
          <div className="animate-fade-up">
            <span className="inline-block rounded-full bg-white/70 px-4 py-1 text-xs font-bold uppercase tracking-widest text-royal">
              Future-ready skincare
            </span>

            <h1 className="mt-5 font-display font-bold uppercase text-royal">
              <span className="block text-display-lg xl:text-display-xl">
                Sophisticated glow,
              </span>
              <span className="block text-display-lg xl:text-display-xl">
                formulas you can trust.
              </span>
            </h1>

            <p className="mt-6 max-w-md text-lg text-royal/90">
              GlowCare is a curated skincare marketplace connecting you with trusted
              brands and independent sellers - filtered by your skin type, your
              ingredients, your budget.
            </p>

            {/* White pill CTA with blue arrow - reference 1 button style. */}
            <div className="mt-8 flex flex-wrap items-center gap-4">
              <Link
                to="/products"
                className="btn-light group min-w-[13rem] justify-between px-3 py-3"
              >
                Explore now
                <span className="flex h-9 w-9 items-center justify-center rounded-full bg-royal text-white transition-transform group-hover:translate-x-1">
                  <ArrowRight className="h-4 w-4" />
                </span>
              </Link>
              <Link to="/products" className="font-semibold text-royal underline-offset-4 hover:underline">
                Explore products
              </Link>
            </div>

            {/* Value bullets - reference 1. */}
            <ul className="mt-9 grid gap-3 sm:grid-cols-2">
              {VALUE_PROPS.map((item) => (
                <li key={item} className="flex items-center gap-3 font-semibold text-royal">
                  <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-white">
                    <Check className="h-4 w-4 text-royal" />
                  </span>
                  {item}
                </li>
              ))}
            </ul>
          </div>

          {/* Organic sky-backed image - reference 1 composition. */}
          <div className="relative mx-auto w-full max-w-xl">
            <div className="shape-organic bg-white/40 p-4">
              <EditorialImage
                src={products[0]?.mainImageUrl}
                alt="GlowCare signature serum"
                shape="organic"
                ratio="aspect-[5/4]"
              />
            </div>
            <div className="absolute -bottom-6 -left-4 rounded-2xl bg-white px-5 py-4 shadow-sm sm:-left-8">
              <p className="font-display text-3xl text-royal">
                {products.length > 0 ? `${products.length}+` : 'Curated'}
              </p>
              <p className="text-sm text-navy/70">editor-approved picks</p>
            </div>
          </div>
        </Container>
      </section>

      {/* ================================================================
          2. POPULAR PRODUCTS - off-white band, blue editorial heading.
      ================================================================ */}
      <section className="py-16">
        <Container>
          <div className="flex items-end justify-between gap-4">
            <h2 className="heading-display uppercase">Popular Products</h2>
            <Link
              to="/products"
              className="hidden items-center gap-1 text-sm font-semibold text-royal hover:underline sm:flex"
            >
              View all <ArrowRight className="h-4 w-4" />
            </Link>
          </div>

          {loading ? (
            <Spinner label="Loading products..." />
          ) : products.length === 0 ? (
            <p className="mt-6 card px-6 py-12 text-center text-navy/60">
              No products have been published yet. Check back soon.
            </p>
          ) : (
            <div className="mt-8 grid grid-cols-2 gap-5 md:grid-cols-3 lg:grid-cols-4">
              {products.map((product) => (
                <ProductCard key={product.id} product={product} />
              ))}
            </div>
          )}
        </Container>
      </section>

      {/* ================================================================
          3. CATEGORIES - sky-blue image tiles on off-white.
      ================================================================ */}
      {categories.length > 0 && (
        <section className="bg-white py-16">
          <Container>
            <h2 className="heading-display uppercase">Shop by Category</h2>
            <div className="mt-8 grid grid-cols-2 gap-5 md:grid-cols-3 lg:grid-cols-5">
              {categories.map((category) => (
                <Link key={category.id} to={`/products?categoryId=${category.id}`} className="group">
                  <div className="aspect-square overflow-hidden rounded-full bg-sky/50 transition-transform duration-300 group-hover:scale-[1.03]">
                    {category.imageUrl ? (
                      <img
                        src={category.imageUrl}
                        alt={category.name}
                        loading="lazy"
                        onError={(e) => {
                          e.currentTarget.style.display = 'none'
                        }}
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <div className="flex h-full w-full items-center justify-center">
                        <span className="font-display text-2xl text-royal/70">{category.name}</span>
                      </div>
                    )}
                  </div>
                  <h3 className="mt-4 text-center font-display text-lg text-royal group-hover:underline">
                    {category.name}
                  </h3>
                </Link>
              ))}
            </div>
          </Container>
        </section>
      )}

      {/* ================================================================
          4. EDITORIAL SPLIT - reference 2: off-white, sky ellipse portrait,
             oversized blue headline, circular stamp badge.
      ================================================================ */}
      <section className="py-16 lg:py-24">
        <Container className="grid items-center gap-12 lg:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)]">
          {/* Sky-blue elliptical portrait - reference 2. */}
          <div className="relative mx-auto w-full max-w-sm">
            <EditorialImage
              src={null}
              alt="GlowCare skincare model"
              shape="ellipse"
              ratio="aspect-[3/4]"
            />
            <span className="absolute -right-2 top-6 rounded-full bg-yellow px-4 py-2 font-display text-sm uppercase tracking-widest text-royal shadow-sm">
              Backed by science
            </span>
          </div>

          <div>
            <h2 className="heading-display uppercase">
              Beauty,
              <br />
              backed by
              <br />
              innovation.
            </h2>

            <p className="mt-6 max-w-lg text-lg text-navy/80">
              Every GlowCare formula is listed by an approved seller, with full
              ingredient and skin-type information up front. No guesswork, no hidden
              formulas - just skincare that actually fits your skin.
            </p>

            <ul className="mt-6 grid gap-3">
              {VALUE_PROPS.map((item) => (
                <li key={item} className="flex items-center gap-3 font-semibold text-navy">
                  <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-yellow">
                    <Check className="h-4 w-4 text-royal" />
                  </span>
                  {item}
                </li>
              ))}
            </ul>

            <div className="mt-10 flex items-center gap-6">
              <Link to="/products" className="btn-primary">
                Shop the range <ArrowRight className="h-4 w-4" />
              </Link>
              <CircularBadge text="EXPLORE ALL PRODUCTS" className="hidden sm:inline-flex" />
            </div>
          </div>
        </Container>
      </section>

      {/* ================================================================
          5. BRANDS - white band, blue heading, yellow hover accents.
      ================================================================ */}
      {brands.length > 0 && (
        <section className="bg-white py-16">
          <Container>
            <h2 className="heading-display uppercase">Featured Brands</h2>
            <div className="mt-8 grid grid-cols-2 gap-5 md:grid-cols-4">
              {brands.map((brand) => (
                <Link
                  key={brand.id}
                  to={`/products?brandId=${brand.id}`}
                  className="flex flex-col items-center justify-center gap-3 rounded-2xl bg-off-white p-8 transition-shadow hover:shadow-lg"
                >
                  {brand.logo ? (
                    <img
                      src={brand.logo}
                      alt={brand.name}
                      onError={(e) => {
                        e.currentTarget.style.display = 'none'
                      }}
                      className="h-16 w-16 rounded-full object-cover"
                    />
                  ) : (
                    <div className="flex h-16 w-16 items-center justify-center rounded-full bg-yellow font-display text-xl text-royal">
                      {brand.name.charAt(0)}
                    </div>
                  )}
                  <h3 className="font-display text-lg text-royal">{brand.name}</h3>
                </Link>
              ))}
            </div>
          </Container>
        </section>
      )}

      {/* ================================================================
          6. SKIN TYPES - primary blue band with yellow card hovers.
      ================================================================ */}
      {skinTypes.length > 0 && (
        <section className="bg-royal py-16 text-white">
          <Container>
            <h2 className="font-display font-bold uppercase">
              <span className="text-display-md text-yellow">Discover by skin type</span>
            </h2>
            <div className="mt-8 grid grid-cols-2 gap-5 md:grid-cols-5">
              {skinTypes.map((skinType) => (
                <Link
                  key={skinType.id}
                  to={`/products?skinTypeId=${skinType.id}`}
                  className="rounded-2xl bg-white/10 p-6 text-center transition-colors hover:bg-yellow hover:text-royal"
                >
                  <h3 className="font-display text-xl">{skinType.name}</h3>
                </Link>
              ))}
            </div>
          </Container>
        </section>
      )}

      {/* ================================================================
          7. PROMO BANNERS - managed by Admin, rendered yellow.
      ================================================================ */}
      {banners.length > 0 && (
        <section className="py-16">
          <Container className="grid gap-5 md:grid-cols-2">
            {banners.map((banner) => (
              <div
                key={banner.id}
                className="relative overflow-hidden rounded-3xl bg-yellow p-10"
              >
                <span className="inline-block rounded-full bg-white/70 px-3 py-1 text-xs font-bold uppercase tracking-widest text-royal">
                  Limited offer
                </span>
                <h3 className="mt-4 heading-panel uppercase">{banner.title}</h3>
                {banner.link && (
                  <Link
                    to={banner.link.startsWith('/') ? banner.link : '/products'}
                    className="btn-light mt-6 px-5 py-2 text-sm"
                  >
                    Shop the offer <ArrowRight className="h-4 w-4" />
                  </Link>
                )}
                {banner.imageUrl && (
                  <img
                    src={banner.imageUrl}
                    alt=""
                    onError={(e) => {
                      e.currentTarget.style.display = 'none'
                    }}
                    className="pointer-events-none absolute -right-8 -top-8 h-36 w-36 rounded-full object-cover opacity-80"
                  />
                )}
              </div>
            ))}
          </Container>
        </section>
      )}

      {/* ================================================================
          8. TRANSPARENCY - reference 1 restated: yellow band, oversized
             blue headline, organic image, white pill CTA.
      ================================================================ */}
      <section className="bg-yellow">
        <Container className="grid items-center gap-12 py-16 lg:grid-cols-2 lg:py-24">
          <div>
            <h2 className="heading-display uppercase">
              Transparency is the
              <br />
              new luxury.
            </h2>
            <p className="mt-6 max-w-md text-lg text-royal/90">
              At GlowCare, luxury never hides behind mystery. Every formula is
              crafted with proven actives, verified and reviewed by the people
              who actually use it.
            </p>

            <Link
              to="/products"
              className="btn-light group mt-8 min-w-[13rem] justify-between px-3 py-3"
            >
              Explore now
              <span className="flex h-9 w-9 items-center justify-center rounded-full bg-royal text-white transition-transform group-hover:translate-x-1">
                <ArrowRight className="h-4 w-4" />
              </span>
            </Link>
          </div>

          <div className="mx-auto w-full max-w-xl">
            <EditorialImage
              src={products[0]?.mainImageUrl}
              alt="Skincare ritual"
              shape="organic"
              ratio="aspect-[4/3]"
            />
          </div>
        </Container>
      </section>
    </div>
  )
}
