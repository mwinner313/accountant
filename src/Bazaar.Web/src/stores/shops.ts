import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import * as repo from '@/db/repositories/shops'
import { flushOutbox } from '@/sync/syncEngine'
import type { CachedShop } from '@/db/db'
const ACTIVE_SHOP_KEY = 'bazaar.activeShop.v1'

export const useShopStore = defineStore('shops', () => {
  const items = ref<CachedShop[]>([])
  const activeShopId = ref<string | null>(localStorage.getItem(ACTIVE_SHOP_KEY))
  const loading = ref(false)
  const error = ref<string | null>(null)

  const activeShop = computed(() => items.value.find(s => s.shopId === activeShopId.value) ?? null)

  function setActive(shopId: string | null) {
    activeShopId.value = shopId
    if (shopId) localStorage.setItem(ACTIVE_SHOP_KEY, shopId)
    else localStorage.removeItem(ACTIVE_SHOP_KEY)
  }

  async function loadLocal() {
    items.value = await repo.getAll()
    if (activeShopId.value && !items.value.some(s => s.shopId === activeShopId.value)) {
      setActive(items.value[0]?.shopId ?? null)
    }
  }

  async function refresh() {
    loading.value = true
    error.value = null
    try {
      await flushOutbox()
      try {
        await repo.pullRemote()
      } catch (e: any) {
        error.value = e?.message ?? 'fetch-failed'
      }
      await loadLocal()
      if (!activeShopId.value && items.value.length) {
        setActive(items.value[0].shopId)
      }
    } finally {
      loading.value = false
    }
  }

  async function create(title: string) {
    const created = await repo.createOptimistic(title)
    await loadLocal()
    if (!activeShopId.value) setActive(created.shopId)
    void flushOutbox().then(() => loadLocal())
    return created
  }

  async function update(shopId: string, title: string) {
    await repo.updateOptimistic(shopId, title)
    await loadLocal()
    void flushOutbox().then(() => loadLocal())
  }

  async function remove(shopId: string) {
    await repo.deleteOptimistic(shopId)
    if (activeShopId.value === shopId) setActive(null)
    await loadLocal()
    void flushOutbox().then(() => loadLocal())
  }

  return {
    items,
    activeShopId,
    activeShop,
    loading,
    error,
    loadLocal,
    refresh,
    setActive,
    create,
    update,
    remove
  }
})
