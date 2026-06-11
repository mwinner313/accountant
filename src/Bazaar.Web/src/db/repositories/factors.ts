import { db, type CachedFactor } from '../db'
import * as outbox from '../outbox'
import * as api from '@/api/endpoints/factors'
import * as counterpartiesRepo from '@/db/repositories/counterparties'

function localId(): string {
  return 'local-' + (crypto.randomUUID ? crypto.randomUUID() : Date.now().toString(36))
}

export async function getAll(
  shopId: string,
  type?: api.FactorTypeValue | null
): Promise<CachedFactor[]> {
  let coll = db.factors.where('shopId').equals(shopId).filter(f => !f._pendingDelete)
  if (type) {
    coll = coll.filter(f => f.type === type)
  }
  return await coll.reverse().sortBy('date')
}

export async function pullRemote(
  shopId: string,
  type?: api.FactorTypeValue | null
): Promise<void> {
  const res = await api.listFactors(shopId, { skip: 0, take: 200, type: type ?? null })
  const now = Date.now()
  await db.transaction('rw', db.factors, async () => {
    const items = res.items.map(f => ({
      ...f,
      shopId,
      _local: false,
      _pendingDelete: false,
      _updatedAt: now
    })) as CachedFactor[]
    await db.factors.bulkPut(items)
  })
}

export async function pullDetail(shopId: string, factorId: string): Promise<void> {
  const detail = await api.getFactor(shopId, factorId)
  await db.factors.update(factorId, { _detail: detail, _updatedAt: Date.now() })
}

export async function createOptimistic(
  shopId: string,
  payload: api.CreateFactorPayload
): Promise<CachedFactor> {
  const id = localId()
  const cp = await db.counterparties.get(payload.counterpartyId)
  const row: CachedFactor = {
    factorId: id,
    counterpartyId: payload.counterpartyId,
    counterpartyFullName: cp?.fullName ?? null,
    type: payload.type,
    notes: payload.notes ?? null,
    date: payload.date,
    isReversed: false,
    itemCount: payload.items.length,
    createdOn: new Date().toISOString(),
    shopId,
    _local: true,
    _updatedAt: Date.now()
  }
  await db.factors.put(row)
  await outbox.enqueue('factor', 'create', { shopId, payload }, { shopId, localId: id })
  return row
}

export async function editOptimistic(
  shopId: string,
  factorId: string,
  payload: api.EditFactorPayload
): Promise<void> {
  const cp = await db.counterparties.get(payload.counterpartyId)
  await db.factors.update(factorId, {
    notes: payload.notes ?? null,
    date: payload.date,
    itemCount: payload.items.length,
    counterpartyId: payload.counterpartyId,
    counterpartyFullName: cp?.fullName ?? null,
    _updatedAt: Date.now()
  })
  await outbox.enqueue(
    'factorEdit',
    'update',
    { shopId, factorId, payload },
    { shopId }
  )
}

export async function applyServerCreated(
  shopId: string,
  localId: string,
  real: api.CreateFactorResult,
  original: api.CreateFactorPayload
): Promise<void> {
  const cpId = await counterpartiesRepo.resolveCounterpartyIdForSync(original.counterpartyId)
  const cp = await db.counterparties.get(cpId)
  await db.transaction('rw', db.factors, async () => {
    await db.factors.delete(localId)
    await db.factors.put({
      factorId: real.factorId,
      counterpartyId: cpId,
      counterpartyFullName: cp?.fullName ?? null,
      type: original.type,
      notes: original.notes ?? null,
      date: original.date,
      isReversed: false,
      itemCount: original.items.length,
      createdOn: new Date().toISOString(),
      shopId,
      _local: false,
      _updatedAt: Date.now()
    })
  })
}
