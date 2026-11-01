import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import path from 'node:path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
  server: {
    port: 5173,
    // The React dev server proxies /api to the backend so we call a same-origin
    // path in development (no CORS headaches). In production the built app uses
    // the absolute VITE_API_URL instead - see src/services/api/client.ts.
    proxy: {
      '/api': {
        target: 'http://localhost:5205',
        changeOrigin: true,
      },
    },
  },
})
