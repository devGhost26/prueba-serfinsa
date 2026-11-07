import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react()],
  // Puerto fijo: el backend solo acepta peticiones de este origen.
  server: { port: 5173, strictPort: true },
})
