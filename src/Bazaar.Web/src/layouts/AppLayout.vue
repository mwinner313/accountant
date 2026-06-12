<script setup lang="ts">
import { useOnline } from '@/composables/useOnline'
import { usePendingSync } from '@/composables/usePendingSync'
import { useShopStore } from '@/stores/shops'
import { useAuthStore } from '@/auth/auth.store'
import { useRouter } from 'vue-router'
import { useConfirm } from '@/composables/useConfirm'
import { useI18n } from 'vue-i18n'
import { computed } from 'vue'
import {
  Box,
  ChevronDown,
  Home,
  LogOut,
  Receipt,
  Settings,
  Store,
  Tags,
  Users
} from '@lucide/vue'

const { t } = useI18n()
const { online } = useOnline()
const { count: pending } = usePendingSync()
const shops = useShopStore()
const auth = useAuthStore()
const router = useRouter()
const confirm = useConfirm()

const activeTitle = computed(() => shops.activeShop?.title ?? t('app.name'))

function pickShop() {
  router.push({ name: 'shops' })
}

function doLogout() {
  confirm.require({
    message: t('auth.logout_confirm'),
    accept: async () => {
      await auth.logout()
      router.replace({ name: 'phone-login' })
    }
  })
}
</script>

<template>
  <div class="app-shell">
    <header class="app-header">
      <button class="link" type="button" @click="pickShop">
        <Store class="size-4" />
        <span class="title">{{ activeTitle }}</span>
        <ChevronDown class="size-3 small" />
      </button>
      <button class="icon-btn" type="button" :title="t('app.logout')" @click="doLogout">
        <LogOut class="size-5" />
      </button>
    </header>

    <div v-if="!online" class="offline-banner">{{ t('app.offline') }}</div>
    <div v-else-if="pending > 0" class="offline-banner">
      {{ t('app.pending_sync', { count: pending }) }}
    </div>

    <main class="app-content">
      <router-view />
    </main>

    <nav class="bottom-nav">
      <router-link :to="{ name: 'dashboard' }">
        <Home />
        <span>{{ t('nav.dashboard') }}</span>
      </router-link>
      <router-link :to="{ name: 'products' }">
        <Box />
        <span>{{ t('nav.products') }}</span>
      </router-link>
      <router-link :to="{ name: 'factors' }">
        <Receipt />
        <span>{{ t('nav.factors') }}</span>
      </router-link>
      <router-link :to="{ name: 'counterparties' }">
        <Users />
        <span>{{ t('nav.counterparties') }}</span>
      </router-link>
      <router-link :to="{ name: 'categories' }">
        <Tags />
        <span>{{ t('nav.categories') }}</span>
      </router-link>
      <router-link :to="{ name: 'product-properties' }">
        <Settings />
        <span>{{ t('app.settings') }}</span>
      </router-link>
    </nav>
  </div>
</template>

<style scoped lang="scss">
.app-header {
  position: sticky;
  top: 0;
  height: var(--app-header-h);
  background: var(--card);
  border-bottom: 1px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-inline: 1rem;
  z-index: 10;

  .link {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    background: transparent;
    border: none;
    padding: 0.25rem 0.5rem;
    font: inherit;
    color: inherit;
    cursor: pointer;

    .title {
      font-weight: 700;
    }
    .small {
      opacity: 0.6;
    }
  }
  .icon-btn {
    background: transparent;
    border: none;
    color: inherit;
    padding: 0.5rem;
    cursor: pointer;
  }
}
</style>
