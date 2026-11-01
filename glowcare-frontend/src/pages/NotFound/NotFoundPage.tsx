import { Link } from 'react-router-dom'

/** 404 page. */
export function NotFoundPage() {
  return (
    <div className="shell flex min-h-[60vh] flex-col items-center justify-center py-16 text-center">
      <p className="font-display text-display-xl text-royal">404</p>
      <h1 className="mt-2 font-display text-3xl text-navy">Page not found</h1>
      <p className="mt-3 max-w-md text-navy/60">
        The page you are looking for does not exist or may have been moved.
      </p>
      <Link to="/" className="btn-primary mt-6">
        Back to home
      </Link>
    </div>
  )
}
