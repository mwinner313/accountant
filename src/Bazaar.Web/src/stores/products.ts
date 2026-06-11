import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as repo from '@/db/repositories/products'
import { flushOutbox } from '@/sync/syncEngine'
import type { CachedProduct } from '@/db/db'
import type { CreateProductPayload, ProductDetailModel } from '@/api/endpoints/products'
export const useProductStore = defineStore('products', () => {
  const items = ref<CachedProduct[]>([])
  const detail = ref<ProductDetailModel | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadLocal(shopId: string, filter: { categoryId?: string | null; search?: string } = {}) {
    items.value = await repo.getAll(shopId, filter)
  }

  async function refresh(shopId: string, filter: { categoryId?: string | null; search?: string } = {}) {
    loading.value = true
    error.value = null
    try {
      await flushOutbox()
      try {
        await repo.pullRemote(shopId, filter)
      } catch (e: any) {
        error.value = e?.message ?? 'fetch-failed'
      }
      await loadLocal(shopId, filter)
    } finally {
      loading.value = false
    }
  }

  async function loadDetail(shopId: string, productId: string) {
    const cached = await repo.getById(productId)
    detail.value = cached?._detail ?? null
    try {
      await repo.pullDetail(shopId, productId)
      const fresh = await repo.getById(productId)
      detail.value = fresh?._detail ?? null
    } catch {
      // offline-ok: keep cached detail
    }
  }

  async function create(shopId: string, payload: CreateProductPayload) {
    const created = await repo.createOptimistic(shopId, payload)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
    return created
  }

  async function update(shopId: string, productId: string, payload: CreateProductPayload) {
    await repo.updateOptimistic(shopId, productId, payload)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
  }

  async function remove(shopId: string, productId: string) {
    await repo.deleteOptimistic(shopId, productId)
    await loadLocal(shopId)
    void flushOutbox().then(() => loadLocal(shopId))
  }

  async function setProperty(
    shopId: string,
    productId: string,
    productPropertyId: string,
    value: string
  ) {
    await repo.setPropertyOptimistic(shopId, productId, productPropertyId, value)
    void flushOutbox().then(() => loadDetail(shopId, productId))
  }

  return { items, detail, loading, error, loadLocal, refresh, loadDetail, create, update, remove, setProperty }
})
