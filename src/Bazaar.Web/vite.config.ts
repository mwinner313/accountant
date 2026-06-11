import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { VitePWA } from 'vite-plugin-pwa'
import Components from 'unplugin-vue-components/vite'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'
import AutoImport from 'unplugin-auto-import/vite'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiUrl = env.VITE_API_URL || 'http://localhost:5108'
  const identityUrl = env.VITE_IDENTITY_URL || 'http://localhost:5209'

  return {
    plugins: [
      vue(),
      AutoImport({
        imports: ['vue', 'vue-router', 'pinia', '@vueuse/core'],
        dts: 'src/auto-imports.d.ts',
        eslintrc: { enabled: false }
      }),
      Components({
        resolvers: [PrimeVueResolver()],
        dts: 'src/components.d.ts',
        dirs: ['src/components']
      }),
      VitePWA({
        registerType: 'autoUpdate',
        injectRegister: 'auto',
        devOptions: { enabled: false },
        includeAssets: ['icons/favicon.ico', 'icons/icon.svg', 'icons/apple-touch-icon-180x180.png'],
        manifest: {
          id: 'com.bazaar.app',
          name: 'بازار - حسابداری فروشگاه',
          short_name: 'بازار',
          description: 'مدیریت موجودی و فاکتورهای فروشگاه شما',
          lang: 'fa-IR',
          dir: 'rtl',
          start_url: '/',
          scope: '/',
          display: 'standalone',
          orientation: 'portrait',
          background_color: '#0f172a',
          theme_color: '#0f172a',
          categories: ['business', 'finance', 'productivity'],
          icons: [
            { src: '/icons/pwa-64x64.png', sizes: '64x64', type: 'image/png' },
            { src: '/icons/pwa-192x192.png', sizes: '192x192', type: 'image/png' },
            { src: '/icons/pwa-512x512.png', sizes: '512x512', type: 'image/png' },
            {
              src: '/icons/maskable-icon-512x512.png',
              sizes: '512x512',
              type: 'image/png',
              purpose: 'maskable'
            }
          ]
        },
        workbox: {
          globPatterns: ['**/*.{js,css,html,svg,png,ico,woff,woff2}'],
          navigateFallback: '/index.html',
          navigateFallbackDenylist: [/^\/api/, /^\/connect/, /^\/.well-known/, /^\/swagger/],
          runtimeCaching: [
            {
              urlPattern: ({ url }) => url.pathname.startsWith('/api/'),
              handler: 'StaleWhileRevalidate',
              method: 'GET',
              options: {
                cacheName: 'bazaar-api-get',
                expiration: { maxEntries: 200, maxAgeSeconds: 60 * 60 * 24 * 7 },
                cacheableResponse: { statuses: [0, 200] }
              }
            },
            {
              urlPattern: ({ url }) => url.pathname.startsWith('/swagger/'),
              handler: 'NetworkFirst',
              options: {
                cacheName: 'bazaar-swagger',
                expiration: { maxEntries: 4, maxAgeSeconds: 60 * 60 * 24 }
              }
            },
            {
              urlPattern: ({ url, request }) =>
                request.method !== 'GET' &&
                (url.pathname.startsWith('/api/') || url.pathname.startsWith('/connect/')),
              handler: 'NetworkOnly',
              method: 'POST',
              options: {
                backgroundSync: {
                  name: 'bazaar-write-queue',
                  options: { maxRetentionTime: 60 * 24 }
                }
              }
            }
          ]
        }
      })
    ],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      }
    },
    server: {
      port: 5173,
      strictPort: false,
      proxy: {
        '/api': { target: apiUrl, changeOrigin: true, secure: false },
        '/swagger': { target: apiUrl, changeOrigin: true, secure: false },
        '/connect': { target: identityUrl, changeOrigin: true, secure: false },
        '/.well-known': { target: identityUrl, changeOrigin: true, secure: false }
      }
    },
    build: {
      target: 'es2022',
      sourcemap: true,
      chunkSizeWarningLimit: 800
    }
  }
})
