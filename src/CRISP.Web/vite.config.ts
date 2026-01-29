import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: process.env.services__webServer__https__0 || 'https://localhost:7001',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  // Use shared Tailwind config from repo root
  css: {
    postcss: {
      plugins: [],
    },
  },
})
