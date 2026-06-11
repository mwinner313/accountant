import { db, type CachedCounterparty } from '../db'
import * as outbox from '../outbox'
import * as api from '@/api/endpoints/counterparties'

function localId(): string {
  return 'local-' + (crypto.randomUUID ? crypto.randomUUID() : Date.now().toString(36))
}

const remapKey = (localId: string) => `cpRemap:${localId}`

export async function getAll(): Promise<CachedCounterparty[]> {
  return await db.counterparties.filter(c => !c._pendingDelete).toArray()
}

export async function getById(counterpartyId: string): Promise<CachedCounterparty | undefined> {
  return await db.counterparties.get(counterpartyId)
}

export async function pullRemote(search?: string | null): Promise<void> {
  const res = await api.listCounterparties({ skip: 0, take: 200, search: search ?? undefined })
  const now = Date.now()
  await db.transaction('rw', db.counterparties, async () => {
    const localOnly = await db.counterparties.filter(c => !!c._local).toArray()
    await db.counterparties.clear()
    if (localOnly.length) await db.counterparties.bulkPut(localOnly)
    await db.counterparties.bulkPut(
      res.items.map(c => ({
        ...c,
        _local: false,
        _pendingDelete: false,
        _updatedAt: now
      }))
    )
  })
}

export async function createOptimistic(
  payload: api.CreateCounterpartyPayload
): Promise<CachedCounterparty> {
  const id = localId()
  const row: CachedCounterparty = {
    counterpartyId: id,
    fullName: payload.fullName,
    _local: true,
    _updatedAt: Date.now()
  }
  await db.counterparties.put(row)
  await outbox.enqueue('counterparty', 'create', { ...payload }, { localId: id })
  return row
}

export async function updateOptimistic(
  counterpartyId: string,
  payload: api.CreateCounterpartyPayload
): Promise<void> {
  await db.counterparties.update(counterpartyId, {
    fullName: payload.fullName,
    _updatedAt: Date.now()
  })
  await outbox.enqueue('counterparty', 'update', { counterpartyId, ...payload }, {})
}

export async function deleteOptimistic(counterpartyId: string): Promise<void> {
  await db.counterparties.update(counterpartyId, { _pendingDelete: true, _updatedAt: Date.now() })
  await outbox.enqueue('counterparty', 'delete', { counterpartyId }, {})
}

export async function applyServerCreated(
  localId: string,
  real: api.CreateCounterpartyResult,
  original: api.CreateCounterpartyPayload
): Promise<void> {
  await db.transaction('rw', [db.counterparties, db.meta], async () => {
    await db.counterparties.delete(localId)
    await db.counterparties.put({
      counterpartyId: real.counterpartyId,
      fullName: original.fullName,
      _local: false,
      _pendingDelete: false,
      _updatedAt: Date.now()
    })
    await db.meta.put({ key: remapKey(localId), value: real.counterpartyId })
  })
}

export async function applyServerDeleted(counterpartyId: string): Promise<void> {
  await db.counterparties.delete(counterpartyId)
}

export async function resolveCounterpartyIdForSync(id: string): Promise<string> {
  if (!id.startsWith('local-')) return id
  const m = await db.meta.get(remapKey(id))
  return (m?.value as string) ?? id
}
