import { db, type CachedProductProperty } from '../db'
import * as outbox from '../outbox'
import * as api from '@/api/endpoints/productProperties'

function localId(): string {
  return 'local-' + (crypto.randomUUID ? crypto.randomUUID() : Date.now().toString(36))
}

export async function getAll(shopId: string): Promise<CachedProductProperty[]> {
  return await db.productProperties
    .where('shopId')
    .equals(shopId)
    .filter(p => !p._pendingDelete)
    .toArray()
}

export async function createOptimistic(
  shopId: string,
  name: string
): Promise<CachedProductProperty> {
  const id = localId()
  const row: CachedProductProperty = {
    productPropertyId: id,
    name,
    shopId,
    _local: true,
    _updatedAt: Date.now()
  }
  await db.productProperties.put(row)
  await outbox.enqueue('productProperty', 'create', { shopId, name }, { shopId, localId: id })
  return row
}

export async function updateOptimistic(
  shopId: string,
  propertyId: string,
  name: string
): Promise<void> {
  await db.productProperties.update(propertyId, { name, _updatedAt: Date.now() })
  await outbox.enqueue(
    'productProperty',
    'update',
    { shopId, propertyId, name },
    { shopId }
  )
}

export async function deleteOptimistic(shopId: string, propertyId: string): Promise<void> {
  await db.productProperties.update(propertyId, {
    _pendingDelete: true,
    _updatedAt: Date.now()
  })
  await outbox.enqueue('productProperty', 'delete', { shopId, propertyId }, { shopId })
}

export async function applyServerCreated(
  shopId: string,
  localId: string,
  real: api.CreateProductPropertyResult
): Promise<void> {
  await db.transaction('rw', db.productProperties, async () => {
    await db.productProperties.delete(localId)
    await db.productProperties.put({
      productPropertyId: real.productPropertyId,
      name: real.name,
      shopId,
      _local: false,
      _updatedAt: Date.now()
    })
  })
}

export async function applyServerDeleted(propertyId: string): Promise<void> {
  await db.productProperties.delete(propertyId)
}
