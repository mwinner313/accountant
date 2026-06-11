import { db, type CachedShop } from '../db'
import * as outbox from '../outbox'
import * as api from '@/api/endpoints/shops'

function localId(): string {
  return 'local-' + (crypto.randomUUID ? crypto.randomUUID() : Date.now().toString(36))
}

export async function getAll(): Promise<CachedShop[]> {
  return await db.shops.filter(s => !s._pendingDelete).toArray()
}

export async function getById(shopId: string): Promise<CachedShop | undefined> {
  return await db.shops.get(shopId)
}

export async function pullRemote(): Promise<void> {
  const res = await api.listShops({ skip: 0, take: 200 })
  const now = Date.now()
  await db.transaction('rw', db.shops, async () => {
    const localOnly = await db.shops.filter(s => !!s._local).toArray()
    await db.shops.clear()
    if (localOnly.length) await db.shops.bulkPut(localOnly)
    await db.shops.bulkPut(
      res.items.map(s => ({ ...s, _local: false, _pendingDelete: false, _updatedAt: now }))
    )
  })
}

export async function createOptimistic(title: string): Promise<CachedShop> {
  const id = localId()
  const now = Date.now()
  const row: CachedShop = {
    shopId: id,
    title,
    createdOn: new Date().toISOString(),
    _local: true,
    _updatedAt: now
  }
  await db.shops.put(row)
  await outbox.enqueue('shop', 'create', { title }, { localId: id })
  return row
}

export async function updateOptimistic(shopId: string, title: string): Promise<void> {
  await db.shops.update(shopId, { title, _updatedAt: Date.now() })
  await outbox.enqueue('shop', 'update', { shopId, title }, { shopId })
}

export async function deleteOptimistic(shopId: string): Promise<void> {
  await db.shops.update(shopId, { _pendingDelete: true, _updatedAt: Date.now() })
  await outbox.enqueue('shop', 'delete', { shopId }, { shopId })
}

export async function applyServerCreated(localId: string, real: api.CreateShopResult): Promise<void> {
  await db.transaction('rw', db.shops, async () => {
    await db.shops.delete(localId)
    await db.shops.put({
      shopId: real.shopId,
      title: real.title,
      createdOn: new Date().toISOString(),
      _local: false,
      _updatedAt: Date.now()
    })
  })
}

export async function applyServerDeleted(shopId: string): Promise<void> {
  await db.shops.delete(shopId)
}
