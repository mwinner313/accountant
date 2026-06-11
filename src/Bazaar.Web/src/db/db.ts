import Dexie, { type Table } from 'dexie'
import type { ShopModel } from '@/api/endpoints/shops'
import type { CategoryModel } from '@/api/endpoints/categories'
import type { ProductDetailModel, ProductModel } from '@/api/endpoints/products'
import type { ProductPropertyModel } from '@/api/endpoints/productProperties'
import type { FactorDetailModel, FactorModel } from '@/api/endpoints/factors'
import type { CounterpartyListModel } from '@/api/endpoints/counterparties'

export type OutboxOp = 'create' | 'update' | 'delete' | 'custom'
export type OutboxStatus = 'pending' | 'failed' | 'syncing'
export type OutboxEntity =
  | 'shop'
  | 'category'
  | 'product'
  | 'productProperty'
  | 'productPropertyValue'
  | 'factor'
  | 'factorEdit'
  | 'counterparty'

export interface OutboxEntry {
  id?: number
  entity: OutboxEntity
  op: OutboxOp
  shopId?: string | null
  /** Local optimistic ID (UUID) used as a placeholder until the server returns a real ID. */
  localId?: string | null
  payload: any
  attempts: number
  status: OutboxStatus
  lastError?: string | null
  createdAt: number
  updatedAt: number
}

export interface MetaEntry {
  key: string
  value: any
}

export interface CachedShop extends ShopModel {
  _ownerId?: string | null
  _local?: boolean
  _pendingDelete?: boolean
  _updatedAt: number
}

export interface CachedCategory extends CategoryModel {
  shopId: string
  _local?: boolean
  _pendingDelete?: boolean
  _updatedAt: number
}

export interface CachedProduct extends ProductModel {
  shopId: string
  _local?: boolean
  _pendingDelete?: boolean
  _updatedAt: number
  /** Full detail (incl. properties) once fetched at least once */
  _detail?: ProductDetailModel | null
}

export interface CachedProductProperty extends ProductPropertyModel {
  shopId: string
  _local?: boolean
  _pendingDelete?: boolean
  _updatedAt: number
}

export interface CachedFactor extends FactorModel {
  shopId: string
  _local?: boolean
  _pendingDelete?: boolean
  _updatedAt: number
  _detail?: FactorDetailModel | null
}

export interface CachedCounterparty extends CounterpartyListModel {
  _local?: boolean
  _pendingDelete?: boolean
  _updatedAt: number
}

class BazaarDb extends Dexie {
  shops!: Table<CachedShop, string>
  categories!: Table<CachedCategory, string>
  products!: Table<CachedProduct, string>
  productProperties!: Table<CachedProductProperty, string>
  factors!: Table<CachedFactor, string>
  counterparties!: Table<CachedCounterparty, string>
  outbox!: Table<OutboxEntry, number>
  meta!: Table<MetaEntry, string>

  constructor() {
    super('bazaar-web')
    this.version(1).stores({
      shops: 'shopId, _ownerId, _updatedAt',
      categories: 'categoryId, shopId, _updatedAt',
      products: 'productId, shopId, categoryId, _updatedAt',
      productProperties: 'productPropertyId, shopId, _updatedAt',
      factors: 'factorId, shopId, type, date, _updatedAt',
      outbox: '++id, entity, status, createdAt',
      meta: '&key'
    })
    this.version(2).stores({
      shops: 'shopId, _ownerId, _updatedAt',
      categories: 'categoryId, shopId, _updatedAt',
      products: 'productId, shopId, categoryId, _updatedAt',
      productProperties: 'productPropertyId, shopId, _updatedAt',
      factors: 'factorId, shopId, type, date, counterpartyId, _updatedAt',
      counterparties: 'counterpartyId, fullName, _updatedAt',
      outbox: '++id, entity, status, createdAt',
      meta: '&key'
    })
  }
}

export const db = new BazaarDb()
