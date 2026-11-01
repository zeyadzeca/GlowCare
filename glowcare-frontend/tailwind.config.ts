import type { Config } from 'tailwindcss'

/**
 * GlowCare design tokens - EXACT palette extracted from the approved
 * visual reference images (the "Orvélia"-style skincare pages).
 *
 * Palette (do not introduce colours outside this family):
 *   Primary Yellow  #FAEB68 - hero, promo banners, highlighted blocks
 *   Primary Blue    #3162AF - headings, buttons, icons, accents
 *   Warm Off-White  #F7F8F1 - page background / light sections
 *   Sky Blue        #93D1F5 - image backgrounds, soft accents
 *   White           #FFFFFF - cards, buttons, clean content areas
 *
 * `navy` is a darker variation OF the primary blue (deep blue ink) used for
 * body text and the footer so paragraphs stay readable - it is not a new
 * palette, just #3162AF taken darker. Likewise `royal-dark`/`yellow-deep`
 * are hover shades of the two primary colours.
 */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // --- Core reference palette ---------------------------------
        yellow: {
          DEFAULT: '#FAEB68', // Primary Yellow
          deep: '#F3DF45', // hover / pressed
          soft: '#FDF6BC', // subtle tint on white
          gold: '#E5C63A', // darker variation - star ratings on white
        },
        royal: {
          DEFAULT: '#3162AF', // Primary Blue
          dark: '#27518F', // hover
          light: '#5A87C9', // lighter variation - subtle accents
        },
        sky: {
          DEFAULT: '#93D1F5', // Sky Blue
          deep: '#6FBDEA', // hover
          soft: '#CFEBFB', // very soft accent / placeholder art
        },
        'off-white': {
          DEFAULT: '#F7F8F1', // Warm Off-White (page background)
          deep: '#ECEEE1', // separators / subtle bands
        },
        // --- Deep variation of Primary Blue (text + dark surfaces) ---
        navy: {
          DEFAULT: '#12294A', // deep blue ink - body text, footer
          deep: '#0B1C33', // darkest shade - hover / pressed
        },
        // Accent colour used ON TOP of the navy/dark surfaces.
        'bright-blue': '#93D1F5',
        white: '#FFFFFF',
      },
      fontFamily: {
        // Oswald = condensed display type for headings, Inter = body text.
        display: ['Oswald', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        // Oversized, bold, condensed editorial headlines (reference proportions).
        'display-xl': ['4.75rem', { lineHeight: '0.92', fontWeight: '700' }],
        'display-lg': ['3.75rem', { lineHeight: '0.95', fontWeight: '700' }],
        'display-md': ['2.75rem', { lineHeight: '1.02', fontWeight: '700' }],
        'display-sm': ['2rem', { lineHeight: '1.1', fontWeight: '700' }],
      },
      maxWidth: {
        shell: '80rem',
      },
      keyframes: {
        'fade-up': {
          '0%': { opacity: '0', transform: 'translateY(12px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
      },
      animation: {
        'fade-up': 'fade-up 0.5s ease-out both',
      },
    },
  },
  plugins: [],
} satisfies Config
