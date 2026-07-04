import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Backend gerçekten /api altında sunuyor (bkz. Program.cs) — üretimde
      // aynı uygulama tarafından tek origin olarak sunulacağı için burada
      // yol yeniden yazma (rewrite) yapılmaz, sadece iletilir.
      '/api': {
        target: 'http://localhost:5269',
        changeOrigin: true,
      },
    },
  },
})
