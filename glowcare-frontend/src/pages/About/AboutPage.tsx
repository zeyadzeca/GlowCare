import { Link } from 'react-router-dom'
import { Check } from 'lucide-react'
import { Container } from '@/components/layout/Container'
import { CircularBadge } from '@/components/common/CircularBadge'
import { EditorialImage } from '@/components/common/EditorialImage'

const VALUES = [
  '100% transparent formulas',
  'Only verified ingredients',
  'Filter by skin type and actives',
  'Approved-seller marketplace',
]

export function AboutPage() {
  return (
    <div>
      <section className="bg-yellow">
        <Container className="grid items-center gap-12 py-16 lg:grid-cols-2 lg:py-24">
          <div>
            <h1 className="heading-display uppercase">
              Transparency is the
              <br />
              new luxury.
            </h1>
            <p className="mt-6 max-w-md text-lg text-royal/90">
              GlowCare is a skincare marketplace for customers who want to know
              what they are putting on their skin, and for sellers who are willing
              to list it plainly.
            </p>
            <Link to="/products" className="btn-light mt-8">
              Shop the marketplace
            </Link>
          </div>
          <EditorialImage src={null} alt="" shape="organic" ratio="aspect-[4/3]" />
        </Container>
      </section>

      <section className="py-16 lg:py-24">
        <Container className="grid items-center gap-12 lg:grid-cols-2">
          <EditorialImage src={null} alt="" shape="ellipse" ratio="aspect-[3/4]" className="mx-auto max-w-sm" />
          <div>
            <h2 className="heading-display uppercase">
              Beauty,
              <br />
              backed by
              <br />
              innovation.
            </h2>
            <p className="mt-6 max-w-lg text-lg text-navy/80">
              Every GlowCare formula is listed by an approved seller, with ingredient
              and skin-type information up front. No guesswork, and no hidden formulas.
            </p>
            <ul className="mt-6 grid gap-3">
              {VALUES.map((item) => (
                <li key={item} className="flex items-center gap-3 font-semibold text-navy">
                  <span className="flex h-6 w-6 items-center justify-center rounded-full bg-yellow">
                    <Check className="h-4 w-4 text-royal" />
                  </span>
                  {item}
                </li>
              ))}
            </ul>
            <div className="mt-10 flex items-center gap-6">
              <Link to="/products" className="btn-primary">Explore products</Link>
              <CircularBadge text="EXPLORE ALL PRODUCTS" className="hidden sm:inline-flex" />
            </div>
          </div>
        </Container>
      </section>
    </div>
  )
}
