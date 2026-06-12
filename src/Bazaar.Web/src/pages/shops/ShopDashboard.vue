<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useShopStore } from '@/stores/shops'
import { useProductStore } from '@/stores/products'
import { useFactorStore } from '@/stores/factors'
import { FactorType } from '@/api/endpoints/factors'
import { toJalali } from '@/i18n/format'
import {
  AlertTriangle,
  ArrowDownLeft,
  ArrowUpRight,
  Box,
  ChevronLeft
} from '@lucide/vue'
import { Badge } from '@/components/ui/badge'

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
      <div class="stat__icon"><Box class="size-5" /></div>
      <div class="stat__body">
        <div class="stat__label">{{ t('nav.products') }}</div>
        <div class="stat__value">{{ productCount }}</div>
      </div>
    </div>
    <div class="stat">
      <div class="stat__icon danger"><AlertTriangle class="size-5" /></div>
      <div class="stat__body">
        <div class="stat__label">{{ t('products.inventory') }} ≤ 0</div>
        <div class="stat__value">{{ lowStock }}</div>
      </div>
    </div>
    <div class="stat">
      <div class="stat__icon success"><ArrowDownLeft class="size-5" /></div>
      <div class="stat__body">
        <div class="stat__label">{{ t('factors.type_buy') }}</div>
        <div class="stat__value">{{ summary.totalBuy }}</div>
      </div>
    </div>
    <div class="stat">
      <div class="stat__icon warning"><ArrowUpRight class="size-5" /></div>
      <div class="stat__body">
        <div class="stat__label">{{ t('factors.type_sell') }}</div>
        <div class="stat__value">{{ summary.totalSell }}</div>
      </div>
    </div>
  </div>

  <h3 class="section-title">{{ t('factors.title') }}</h3>
  <EmptyState
    v-if="recentFactors.length === 0"
    icon="receipt"
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
        <ArrowDownLeft v-if="f.type === FactorType.Buy" class="size-5" />
        <ArrowUpRight v-else class="size-5" />
      </div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ f.type === FactorType.Buy ? t('factors.type_buy') : t('factors.type_sell') }}
          <Badge v-if="f.isReversed" variant="destructive">{{ t('factors.reversed') }}</Badge>
        </div>
        <div class="list-card__sub">
          {{ toJalali(f.date) }} • {{ t('factors.item_count', { count: f.itemCount }) }}
        </div>
      </div>
      <ChevronLeft class="size-5 muted" />
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
  background: var(--card);
  border: 1px solid var(--border);
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
    background: color-mix(in oklch, var(--primary) 15%, transparent);
    color: var(--primary);

    &.danger {
      background: oklch(0.95 0.05 25);
      color: oklch(0.55 0.2 25);
    }
    &.success {
      background: oklch(0.95 0.05 145);
      color: oklch(0.45 0.15 145);
    }
    &.warning {
      background: oklch(0.96 0.05 95);
      color: oklch(0.5 0.12 85);
    }
  }
  &__label {
    font-size: 0.8rem;
    color: var(--muted-foreground);
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
  background: oklch(0.96 0.05 95);
  color: oklch(0.45 0.1 85);

  &.buy {
    background: oklch(0.95 0.05 145);
    color: oklch(0.42 0.14 145);
  }
}
</style>
