import * as outbox from '@/db/outbox'
import { db } from '@/db/db'
import { isDevBypassEnabled } from '@/auth/devBypass'

import * as shopsApi from '@/api/endpoints/shops'
import * as categoriesApi from '@/api/endpoints/categories'
import * as productsApi from '@/api/endpoints/products'
import * as productPropsApi from '@/api/endpoints/productProperties'
import * as factorsApi from '@/api/endpoints/factors'
import * as counterpartiesApi from '@/api/endpoints/counterparties'

import * as shopsRepo from '@/db/repositories/shops'
import * as categoriesRepo from '@/db/repositories/categories'
import * as productsRepo from '@/db/repositories/products'
import * as productPropsRepo from '@/db/repositories/productProperties'
import * as factorsRepo from '@/db/repositories/factors'
import * as counterpartiesRepo from '@/db/repositories/counterparties'

let running = false
let timer: number | null = null

export interface SyncResult {
  drained: number
  failed: number
}

export async function flushOutbox(): Promise<SyncResult> {
  if (running) return { drained: 0, failed: 0 }
  if (isDevBypassEnabled()) return { drained: 0, failed: 0 }
  if (typeof navigator !== 'undefined' && navigator.onLine === false) {
    return { drained: 0, failed: 0 }
  }
  running = true
  let drained = 0
  let failed = 0
  try {
    const entries = await outbox.listPending()
    for (const entry of entries) {
      try {
        await outbox.markSyncing(entry.id!)
        await processEntry(entry)
        await outbox.remove(entry.id!)
        drained++
      } catch (err: any) {
        const message = err?.response?.data?.message ?? err?.message ?? 'unknown'
        await outbox.markFailed(entry.id!, String(message))
        failed++
        // Stop draining on 5xx, keep going on 4xx (treat as fatal for the entry).
        const status = err?.response?.status
        if (typeof status === 'number' && status >= 500) break
      }
    }
  } finally {
    running = false
  }
  return { drained, failed }
}

async function processEntry(entry: import('@/db/db').OutboxEntry): Promise<void> {
  const { entity, op, payload } = entry
  switch (entity) {
    case 'shop':
      if (op === 'create') {
        const result = await shopsApi.createShop(payload.title)
        if (entry.localId) await shopsRepo.applyServerCreated(entry.localId, result)
      } else if (op === 'update') {
        await shopsApi.updateShop(payload.shopId, payload.title)
      } else if (op === 'delete') {
        await shopsApi.deleteShop(payload.shopId)
        await shopsRepo.applyServerDeleted(payload.shopId)
      }
      return
    case 'category':
      if (op === 'create') {
        const result = await categoriesApi.createCategory(payload.shopId, payload.name)
        if (entry.localId) await categoriesRepo.applyServerCreated(payload.shopId, entry.localId, result)
      } else if (op === 'update') {
        await categoriesApi.updateCategory(payload.shopId, payload.categoryId, payload.name)
      } else if (op === 'delete') {
        await categoriesApi.deleteCategory(payload.shopId, payload.categoryId)
        await categoriesRepo.applyServerDeleted(payload.categoryId)
      }
      return
    case 'product':
      if (op === 'create') {
        const result = await productsApi.createProduct(payload.shopId, payload.payload)
        if (entry.localId)
          await productsRepo.applyServerCreated(payload.shopId, entry.localId, result, payload.payload)
      } else if (op === 'update') {
        await productsApi.updateProduct(payload.shopId, payload.productId, payload.payload)
      } else if (op === 'delete') {
        await productsApi.deleteProduct(payload.shopId, payload.productId)
        await productsRepo.applyServerDeleted(payload.productId)
      }
      return
    case 'productProperty':
      if (op === 'create') {
        const result = await productPropsApi.createProductProperty(payload.shopId, payload.name)
        if (entry.localId)
          await productPropsRepo.applyServerCreated(payload.shopId, entry.localId, result)
      } else if (op === 'update') {
        await productPropsApi.updateProductProperty(payload.shopId, payload.propertyId, payload.name)
      } else if (op === 'delete') {
        await productPropsApi.deleteProductProperty(payload.shopId, payload.propertyId)
        await productPropsRepo.applyServerDeleted(payload.propertyId)
      }
      return
    case 'productPropertyValue':
      await productsApi.setProductPropertyValue(
        payload.shopId,
        payload.productId,
        payload.productPropertyId,
        payload.value
      )
      return
    case 'counterparty':
      if (op === 'create') {
        const result = await counterpartiesApi.createCounterparty({
          fullName: payload.fullName,
          phones: payload.phones ?? [],
          bankAccounts: payload.bankAccounts ?? []
        })
        if (entry.localId)
          await counterpartiesRepo.applyServerCreated(entry.localId, result, {
            fullName: payload.fullName,
            phones: payload.phones ?? [],
            bankAccounts: payload.bankAccounts ?? []
          })
      } else if (op === 'update') {
        await counterpartiesApi.updateCounterparty(payload.counterpartyId, {
          fullName: payload.fullName,
          phones: payload.phones,
          bankAccounts: payload.bankAccounts
        })
      } else if (op === 'delete') {
        await counterpartiesApi.deleteCounterparty(payload.counterpartyId)
        await counterpartiesRepo.applyServerDeleted(payload.counterpartyId)
      }
      return
    case 'factor':
      if (op === 'create') {
        const factorPayload = {
          ...payload.payload,
          counterpartyId: await counterpartiesRepo.resolveCounterpartyIdForSync(
            payload.payload.counterpartyId
          )
        }
        const result = await factorsApi.createFactor(payload.shopId, factorPayload)
        if (entry.localId)
          await factorsRepo.applyServerCreated(payload.shopId, entry.localId, result, payload.payload)
      }
      return
    case 'factorEdit': {
      const editPayload = {
        ...payload.payload,
        counterpartyId: await counterpartiesRepo.resolveCounterpartyIdForSync(
          payload.payload.counterpartyId
        )
      }
      await factorsApi.editFactor(payload.shopId, payload.factorId, editPayload)
      return
    }
  }
}

export function startBackgroundSync(): void {
  if (timer !== null) return
  if (typeof window === 'undefined') return
  void flushOutbox()
  window.addEventListener('online', () => void flushOutbox())
  timer = window.setInterval(() => void flushOutbox(), 30_000)
}

export function stopBackgroundSync(): void {
  if (timer !== null) {
    window.clearInterval(timer)
    timer = null
  }
}

export async function clearAllLocalData(): Promise<void> {
  await db.transaction(
    'rw',
    [
      db.shops,
      db.categories,
      db.products,
      db.productProperties,
      db.factors,
      db.counterparties,
      db.outbox,
      db.meta
    ],
    async () => {
      await db.shops.clear()
      await db.categories.clear()
      await db.products.clear()
      await db.productProperties.clear()
      await db.factors.clear()
      await db.counterparties.clear()
      await db.outbox.clear()
      await db.meta.clear()
    }
  )
}
