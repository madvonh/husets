import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig(({ mode, command }) => {
  const env = loadEnv(mode, '.', 'VITE_')
  const apiProxyTarget = env.VITE_API_PROXY_TARGET

  if (command === 'serve' && !apiProxyTarget) {
    throw new Error('Missing VITE_API_PROXY_TARGET. Set it in your environment or .env file.')
  }

  return {
    plugins: [react()],
    server: {
      port: 5173,
      open: true,
      proxy: {
        '/api': {
          target: apiProxyTarget || '',
          changeOrigin: true,
          secure: false,
          rewrite: (path) => path.replace(/^\/api/, ''),
        },
      },
    },
    build: {
      outDir: 'dist',
      sourcemap: true,
    },
  }
})
