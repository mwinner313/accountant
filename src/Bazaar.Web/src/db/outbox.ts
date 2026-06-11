import { db, type OutboxEntity, type OutboxEntry, type OutboxOp } from './db'

export async function enqueue(
  entity: OutboxEntity,
  op: OutboxOp,
  payload: any,
  opts: { shopId?: string | null; localId?: string | null } = {}
): Promise<number> {
  const now = Date.now()
  const id = await db.outbox.add({
    entity,
    op,
    shopId: opts.shopId ?? null,
    localId: opts.localId ?? null,
    payload,
    attempts: 0,
    status: 'pending',
    lastError: null,
    createdAt: now,
    updatedAt: now
  })
  return id as number
}

export async function listPending(): Promise<OutboxEntry[]> {
  return await db.outbox.where('status').anyOf(['pending', 'failed']).sortBy('createdAt')
}

export async function markSyncing(id: number) {
  await db.outbox.update(id, { status: 'syncing', updatedAt: Date.now() })
}

export async function markFailed(id: number, error: string) {
  const entry = await db.outbox.get(id)
  if (!entry) return
  await db.outbox.update(id, {
    status: 'failed',
    lastError: error,
    attempts: (entry.attempts ?? 0) + 1,
    updatedAt: Date.now()
  })
}

export async function remove(id: number) {
  await db.outbox.delete(id)
}

export async function pendingCount(): Promise<number> {
  return await db.outbox.where('status').anyOf(['pending', 'failed']).count()
}
