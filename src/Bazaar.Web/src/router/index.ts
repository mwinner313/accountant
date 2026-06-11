import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/auth/auth.store'
import { useShopStore } from '@/stores/shops'

const routes: RouteRecordRaw[] = [
  {
    path: '/auth',
    component: () => import('@/layouts/AuthLayout.vue'),
    meta: { public: true },
    children: [
      { path: '', redirect: '/auth/phone' },
      { path: 'phone', name: 'phone-login', component: () => import('@/pages/auth/PhoneLogin.vue') },
      { path: 'otp', name: 'otp-verify', component: () => import('@/pages/auth/OtpVerify.vue') }
    ]
  },
  {
    path: '/shops',
    component: () => import('@/layouts/AppLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '', name: 'shops', component: () => import('@/pages/shops/ShopsList.vue') }
    ]
  },
  {
    path: '/',
    component: () => import('@/layouts/AppLayout.vue'),
    meta: { requiresAuth: true, requiresShop: true },
    children: [
      { path: '', redirect: '/dashboard' },
      { path: 'dashboard', name: 'dashboard', component: () => import('@/pages/shops/ShopDashboard.vue') },
      { path: 'products', name: 'products', component: () => import('@/pages/products/ProductsList.vue') },
      { path: 'products/new', name: 'product-new', component: () => import('@/pages/products/ProductEdit.vue') },
      { path: 'products/:id', name: 'product-edit', component: () => import('@/pages/products/ProductEdit.vue'), props: true },
      { path: 'categories', name: 'categories', component: () => import('@/pages/categories/CategoriesList.vue') },
      { path: 'factors', name: 'factors', component: () => import('@/pages/factors/FactorsList.vue') },
      { path: 'factors/new', name: 'factor-new', component: () => import('@/pages/factors/FactorEdit.vue') },
      { path: 'factors/:id', name: 'factor-edit', component: () => import('@/pages/factors/FactorEdit.vue'), props: true },
      { path: 'settings/properties', name: 'product-properties', component: () => import('@/pages/settings/ProductPropertiesList.vue') },
      {
        path: 'counterparties',
        name: 'counterparties',
        component: () => import('@/pages/counterparties/CounterpartiesList.vue')
      },
      {
        path: 'counterparties/:id',
        name: 'counterparty-detail',
        component: () => import('@/pages/counterparties/CounterpartyDetail.vue')
      }
    ]
  },
  { path: '/:pathMatch(.*)*', redirect: '/' }
]

export const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async to => {
  const auth = useAuthStore()
  if (!auth.ready) await auth.hydrate()

  const isPublic = to.matched.some(r => r.meta.public)
  if (!isPublic && !auth.isAuthenticated) {
    return { name: 'phone-login' }
  }
  if (isPublic && auth.isAuthenticated) {
    return { path: '/' }
  }

  if (to.matched.some(r => r.meta.requiresShop)) {
    const shops = useShopStore()
    if (!shops.activeShopId) {
      await shops.loadLocal()
      if (!shops.activeShopId) {
        return { name: 'shops' }
      }
    }
  }
  return true
})
