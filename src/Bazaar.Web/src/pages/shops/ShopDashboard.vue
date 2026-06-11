<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useShopStore } from '@/stores/shops'
import { useProductStore } from '@/stores/products'
import { useFactorStore } from '@/stores/factors'
import { FactorType } from '@/api/endpoints/factors'
import { toJalali } from '@/i18n/format'

const { t } = useI18n()
const shops = useShopStore()
const products = useProductStore()
const factors = useFactorStore()

const ready = ref(false)

const productCount = computed(() => products.items.length)
const lowStock = computed(() => products.items.filter(p => Number(p.inventoryAmount) <= 0).length)

const recentFactors = computed(() => factors.items.slice(0, 5))

const summary = computed(() => {
  let totalSell = 0
  let totalBuy = 0
  for (const f of factors.items) {
    if (f.isReversed) continue
    // ItemCount is the only number we have on list. Real money lives in the detail model.
    // Show the count breakdown here; per-factor total can be browsed via FactorEdit.
    if (f.type === FactorType.Sell) totalSell += f.itemCount
    if (f.type === FactorType.Buy) totalBuy += f.itemCount
  }
  return { totalSell, totalBuy }
})

onMounted(async () => {
  const shopId = shops.activeShopId
  if (!shopId) return
  await Promise.all([products.loadLocal(shopId), factors.loadLocal(shopId)])
  void Promise.all([products.refresh(shopId), factors.refresh(shopId)]).finally(() => {
    ready.value = true
  })
})
</script>

<template>
  <PageHeader :title="t('nav.dashboard')" :subtitle="shops.activeShop?.title" />

  <div class="cards">
    <div class="stat">
      <div class="stat__icon"><i class="pi pi-box" /></div>
      <div class="stat__body">
        <div class="stat__label">{{ t('nav.products') }}</div>
        <div class="stat__value">{{ productCount }}</div>
      </div>
    </div>
    <div class="stat">
      <div class="stat__icon danger"><i class="pi pi-exclamation-triangle" /></div>
      <div class="stat__body">
        <div class="stat__label">{{ t('products.inventory') }} ≤ 0</div>
        <div class="stat__value">{{ lowStock }}</div>
      </div>
    </div>
    <div class="stat">
      <div class="stat__icon success"><i class="pi pi-arrow-down-left" /></div>
      <div class="stat__body">
        <div class="stat__label">{{ t('factors.type_buy') }}</div>
        <div class="stat__value">{{ summary.totalBuy }}</div>
      </div>
    </div>
    <div class="stat">
      <div class="stat__icon warning"><i class="pi pi-arrow-up-right" /></div>
      <div class="stat__body">
        <div class="stat__label">{{ t('factors.type_sell') }}</div>
        <div class="stat__value">{{ summary.totalSell }}</div>
      </div>
    </div>
  </div>

  <h3 class="section-title">{{ t('factors.title') }}</h3>
  <EmptyState
    v-if="recentFactors.length === 0"
    icon="pi-receipt"
    :hint="t('factors.no_factors')"
  />
  <div v-else>
    <router-link
      v-for="f in recentFactors"
      :key="f.factorId"
      :to="{ name: 'factor-edit', params: { id: f.factorId } }"
      class="list-card"
    >
      <div class="avatar" :class="{ buy: f.type === FactorType.Buy }">
        <i :class="['pi', f.type === FactorType.Buy ? 'pi-arrow-down-left' : 'pi-arrow-up-right']" />
      </div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ f.type === FactorType.Buy ? t('factors.type_buy') : t('factors.type_sell') }}
          <Tag v-if="f.isReversed" :value="t('factors.reversed')" severity="danger" />
        </div>
        <div class="list-card__sub">
          {{ toJalali(f.date) }} • {{ t('factors.item_count', { count: f.itemCount }) }}
        </div>
      </div>
      <i class="pi pi-angle-left muted" />
    </router-link>
  </div>
</template>

<style scoped lang="scss">
.cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}
.stat {
  background: var(--p-content-background);
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--app-radius);
  padding: 0.85rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;

  &__icon {
    width: 40px;
    height: 40px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--p-primary-100, #e0e7ff);
    color: var(--p-primary-color);

    &.danger {
      background: var(--p-red-100, #fee2e2);
      color: var(--p-red-600, #dc2626);
    }
    &.success {
      background: var(--p-green-100, #dcfce7);
      color: var(--p-green-600, #16a34a);
    }
    &.warning {
      background: var(--p-yellow-100, #fef9c3);
      color: var(--p-yellow-700, #a16207);
    }
  }
  &__label {
    font-size: 0.8rem;
    color: var(--p-text-muted-color);
  }
  &__value {
    font-size: 1.25rem;
    font-weight: 700;
  }
}
.section-title {
  margin: 0 0 0.75rem;
  font-size: 1rem;
  font-weight: 700;
}
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--p-yellow-100, #fef9c3);
  color: var(--p-yellow-800, #854d0e);

  &.buy {
    background: var(--p-green-100, #dcfce7);
    color: var(--p-green-700, #15803d);
  }
}
</style>
