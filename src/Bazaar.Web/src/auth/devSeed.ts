import { db, type CachedCategory, type CachedCounterparty, type CachedFactor, type CachedProduct, type CachedProductProperty, type CachedShop } from '@/db/db'
import { FactorType } from '@/api/endpoints/factors'

/** Stable UUIDs so re-running the seed updates instead of duplicating. */
const IDS = {
  user: 'dev-00000000-0000-0000-0000-000000000001',
  shop1: 'dev-shop-aaaaaaaa-aaaa-4aaa-aaaa-000000000001',
  shop2: 'dev-shop-bbbbbbbb-bbbb-4bbb-bbbb-000000000002',
  catFruit: 'dev-cat-11111111-0001',
  catVeg: 'dev-cat-11111111-0002',
  catNuts: 'dev-cat-11111111-0003',
  prop1: 'dev-prop-22222222-0001',
  prop2: 'dev-prop-22222222-0002',
  prop3: 'dev-prop-22222222-0003',
  pApple: 'dev-prod-33333333-0001',
  pBanana: 'dev-prod-33333333-0002',
  pCucumber: 'dev-prod-33333333-0003',
  pTomato: 'dev-prod-33333333-0004',
  pAlmond: 'dev-prod-33333333-0005',
  pWalnut: 'dev-prod-33333333-0006',
  pPotato: 'dev-prod-33333333-0007',
  pOnion: 'dev-prod-33333333-0008',
  f1: 'dev-fact-44444444-0001',
  f2: 'dev-fact-44444444-0002',
  f3: 'dev-fact-44444444-0003',
  f4: 'dev-fact-44444444-0004',
  f5: 'dev-fact-44444444-0005',
  cpWholesale: 'dev-cp-55555555-0001',
  cpRetail: 'dev-cp-55555555-0002'
}

export const DEV_USER_ID = IDS.user
export const DEV_USER_PHONE = '+989120000000'

export async function seedDevData(): Promise<{ activeShopId: string }> {
  const now = Date.now()
  const today = new Date()
  const dayMs = 86_400_000

  const shops: CachedShop[] = [
    {
      shopId: IDS.shop1,
      title: 'سوپرمارکت رفاه',
      createdOn: new Date(now - 30 * dayMs).toISOString(),
      _ownerId: IDS.user,
      _local: false,
      _updatedAt: now
    },
    {
      shopId: IDS.shop2,
      title: 'فروشگاه میوه و سبزی محله',
      createdOn: new Date(now - 10 * dayMs).toISOString(),
      _ownerId: IDS.user,
      _local: false,
      _updatedAt: now
    }
  ]

  const categories: CachedCategory[] = [
    { categoryId: IDS.catFruit, name: 'میوه', shopId: IDS.shop1, _local: false, _updatedAt: now },
    { categoryId: IDS.catVeg, name: 'سبزیجات', shopId: IDS.shop1, _local: false, _updatedAt: now },
    { categoryId: IDS.catNuts, name: 'خشکبار', shopId: IDS.shop1, _local: false, _updatedAt: now }
  ]

  const properties: CachedProductProperty[] = [
    { productPropertyId: IDS.prop1, name: 'تولیدکننده', shopId: IDS.shop1, _local: false, _updatedAt: now },
    { productPropertyId: IDS.prop2, name: 'کشور مبدا', shopId: IDS.shop1, _local: false, _updatedAt: now },
    { productPropertyId: IDS.prop3, name: 'بسته‌بندی', shopId: IDS.shop1, _local: false, _updatedAt: now }
  ]

  const products: CachedProduct[] = [
    {
      productId: IDS.pApple, name: 'سیب قرمز', unit: 'کیلوگرم', picture: null,
      sellPrice: 65_000, buyPrice: 50_000, inventoryAmount: 42, categoryId: IDS.catFruit,
      shopId: IDS.shop1, _local: false, _updatedAt: now
    },
    {
      productId: IDS.pBanana, name: 'موز', unit: 'کیلوگرم', picture: null,
      sellPrice: 95_000, buyPrice: 75_000, inventoryAmount: 18, categoryId: IDS.catFruit,
      shopId: IDS.shop1, _local: false, _updatedAt: now
    },
    {
      productId: IDS.pCucumber, name: 'خیار', unit: 'کیلوگرم', picture: null,
      sellPrice: 28_000, buyPrice: 22_000, inventoryAmount: 30, categoryId: IDS.catVeg,
      shopId: IDS.shop1, _local: false, _updatedAt: now
    },
    {
      productId: IDS.pTomato, name: 'گوجه‌فرنگی', unit: 'کیلوگرم', picture: null,
      sellPrice: 32_000, buyPrice: 25_000, inventoryAmount: 0, categoryId: IDS.catVeg,
      shopId: IDS.shop1, _local: false, _updatedAt: now
    },
    {
      productId: IDS.pAlmond, name: 'بادام درختی', unit: 'کیلوگرم', picture: null,
      sellPrice: 850_000, buyPrice: 720_000, inventoryAmount: 6.5, categoryId: IDS.catNuts,
      shopId: IDS.shop1, _local: false, _updatedAt: now
    },
    {
      productId: IDS.pWalnut, name: 'گردو', unit: 'کیلوگرم', picture: null,
      sellPrice: 690_000, buyPrice: 590_000, inventoryAmount: 9.2, categoryId: IDS.catNuts,
      shopId: IDS.shop1, _local: false, _updatedAt: now
    },
    {
      productId: IDS.pPotato, name: 'سیب‌زمینی', unit: 'کیلوگرم', picture: null,
      sellPrice: 24_000, buyPrice: 18_000, inventoryAmount: 120, categoryId: IDS.catVeg,
      shopId: IDS.shop1, _local: false, _updatedAt: now
    },
    {
      productId: IDS.pOnion, name: 'پیاز', unit: 'کیلوگرم', picture: null,
      sellPrice: 19_000, buyPrice: 14_000, inventoryAmount: 80, categoryId: IDS.catVeg,
      shopId: IDS.shop1, _local: false, _updatedAt: now
    }
  ]

  const counterparties: CachedCounterparty[] = [
    {
      counterpartyId: IDS.cpWholesale,
      fullName: 'عمده‌فروش میوه و تره‌بار',
      _local: false,
      _pendingDelete: false,
      _updatedAt: now
    },
    {
      counterpartyId: IDS.cpRetail,
      fullName: 'مشتری نمونه',
      _local: false,
      _pendingDelete: false,
      _updatedAt: now
    }
  ]

  const factors: CachedFactor[] = [
    {
      factorId: IDS.f1,
      type: FactorType.Buy,
      notes: 'خرید هفتگی از بازار میوه و تره‌بار',
      date: new Date(today.getTime() - 7 * dayMs).toISOString(),
      isReversed: false,
      itemCount: 3,
      createdOn: new Date(today.getTime() - 7 * dayMs).toISOString(),
      shopId: IDS.shop1,
      _local: false,
      _updatedAt: now,
      counterpartyId: IDS.cpWholesale,
      counterpartyFullName: 'عمده‌فروش میوه و تره‌بار',
      _detail: {
        factorId: IDS.f1,
        shopId: IDS.shop1,
        type: FactorType.Buy,
        counterpartyId: IDS.cpWholesale,
        counterpartyFullName: 'عمده‌فروش میوه و تره‌بار',
        notes: 'خرید هفتگی از بازار میوه و تره‌بار',
        date: new Date(today.getTime() - 7 * dayMs).toISOString(),
        isReversed: false,
        createdOn: new Date(today.getTime() - 7 * dayMs).toISOString(),
        items: [
          { productId: IDS.pApple, productName: 'سیب قرمز', amount: 50, unitPrice: 50_000, total: 2_500_000 },
          { productId: IDS.pBanana, productName: 'موز', amount: 25, unitPrice: 75_000, total: 1_875_000 },
          { productId: IDS.pCucumber, productName: 'خیار', amount: 40, unitPrice: 22_000, total: 880_000 }
        ]
      }
    },
    {
      factorId: IDS.f2,
      type: FactorType.Sell,
      notes: null,
      date: new Date(today.getTime() - 3 * dayMs).toISOString(),
      isReversed: false,
      itemCount: 2,
      createdOn: new Date(today.getTime() - 3 * dayMs).toISOString(),
      shopId: IDS.shop1,
      _local: false,
      _updatedAt: now,
      counterpartyId: IDS.cpRetail,
      counterpartyFullName: 'مشتری نمونه',
      _detail: {
        factorId: IDS.f2,
        shopId: IDS.shop1,
        type: FactorType.Sell,
        counterpartyId: IDS.cpRetail,
        counterpartyFullName: 'مشتری نمونه',
        notes: null,
        date: new Date(today.getTime() - 3 * dayMs).toISOString(),
        isReversed: false,
        createdOn: new Date(today.getTime() - 3 * dayMs).toISOString(),
        items: [
          { productId: IDS.pApple, productName: 'سیب قرمز', amount: 8, unitPrice: 65_000, total: 520_000 },
          { productId: IDS.pBanana, productName: 'موز', amount: 7, unitPrice: 95_000, total: 665_000 }
        ]
      }
    },
    {
      factorId: IDS.f3,
      type: FactorType.Buy,
      notes: 'خرید خشکبار از بازار تجریش',
      date: new Date(today.getTime() - 2 * dayMs).toISOString(),
      isReversed: false,
      itemCount: 2,
      createdOn: new Date(today.getTime() - 2 * dayMs).toISOString(),
      shopId: IDS.shop1,
      _local: false,
      _updatedAt: now,
      counterpartyId: IDS.cpWholesale,
      counterpartyFullName: 'عمده‌فروش میوه و تره‌بار',
      _detail: {
        factorId: IDS.f3,
        shopId: IDS.shop1,
        type: FactorType.Buy,
        counterpartyId: IDS.cpWholesale,
        counterpartyFullName: 'عمده‌فروش میوه و تره‌بار',
        notes: 'خرید خشکبار از بازار تجریش',
        date: new Date(today.getTime() - 2 * dayMs).toISOString(),
        isReversed: false,
        createdOn: new Date(today.getTime() - 2 * dayMs).toISOString(),
        items: [
          { productId: IDS.pAlmond, productName: 'بادام درختی', amount: 5, unitPrice: 720_000, total: 3_600_000 },
          { productId: IDS.pWalnut, productName: 'گردو', amount: 8, unitPrice: 590_000, total: 4_720_000 }
        ]
      }
    },
    {
      factorId: IDS.f4,
      type: FactorType.Sell,
      notes: 'فروش به مشتری دائمی',
      date: new Date(today.getTime() - 1 * dayMs).toISOString(),
      isReversed: false,
      itemCount: 4,
      createdOn: new Date(today.getTime() - 1 * dayMs).toISOString(),
      shopId: IDS.shop1,
      _local: false,
      _updatedAt: now,
      counterpartyId: IDS.cpRetail,
      counterpartyFullName: 'مشتری نمونه',
      _detail: {
        factorId: IDS.f4,
        shopId: IDS.shop1,
        type: FactorType.Sell,
        counterpartyId: IDS.cpRetail,
        counterpartyFullName: 'مشتری نمونه',
        notes: 'فروش به مشتری دائمی',
        date: new Date(today.getTime() - 1 * dayMs).toISOString(),
        isReversed: false,
        createdOn: new Date(today.getTime() - 1 * dayMs).toISOString(),
        items: [
          { productId: IDS.pCucumber, productName: 'خیار', amount: 3, unitPrice: 28_000, total: 84_000 },
          { productId: IDS.pPotato, productName: 'سیب‌زمینی', amount: 5, unitPrice: 24_000, total: 120_000 },
          { productId: IDS.pOnion, productName: 'پیاز', amount: 4, unitPrice: 19_000, total: 76_000 },
          { productId: IDS.pAlmond, productName: 'بادام درختی', amount: 0.5, unitPrice: 850_000, total: 425_000 }
        ]
      }
    },
    {
      factorId: IDS.f5,
      type: FactorType.Sell,
      notes: 'پرداخت نقدی',
      date: new Date(today.getTime() - 4 * 3600_000).toISOString(),
      isReversed: false,
      itemCount: 1,
      createdOn: new Date(today.getTime() - 4 * 3600_000).toISOString(),
      shopId: IDS.shop1,
      _local: false,
      _updatedAt: now,
      counterpartyId: IDS.cpRetail,
      counterpartyFullName: 'مشتری نمونه',
      _detail: {
        factorId: IDS.f5,
        shopId: IDS.shop1,
        type: FactorType.Sell,
        counterpartyId: IDS.cpRetail,
        counterpartyFullName: 'مشتری نمونه',
        notes: 'پرداخت نقدی',
        date: new Date(today.getTime() - 4 * 3600_000).toISOString(),
        isReversed: false,
        createdOn: new Date(today.getTime() - 4 * 3600_000).toISOString(),
        items: [
          { productId: IDS.pApple, productName: 'سیب قرمز', amount: 2, unitPrice: 65_000, total: 130_000 }
        ]
      }
    }
  ]

  await db.transaction(
    'rw',
    [db.shops, db.categories, db.products, db.productProperties, db.counterparties, db.factors],
    async () => {
      await db.shops.bulkPut(shops)
      await db.categories.bulkPut(categories)
      await db.products.bulkPut(products)
      await db.productProperties.bulkPut(properties)
      await db.counterparties.bulkPut(counterparties)
      await db.factors.bulkPut(factors)
    }
  )

  return { activeShopId: IDS.shop1 }
}

export async function clearDevData(): Promise<void> {
  await db.transaction(
    'rw',
    [db.shops, db.categories, db.products, db.productProperties, db.counterparties, db.factors, db.outbox],
    async () => {
      await db.shops.clear()
      await db.categories.clear()
      await db.products.clear()
      await db.productProperties.clear()
      await db.counterparties.clear()
      await db.factors.clear()
      await db.outbox.clear()
    }
  )
}
