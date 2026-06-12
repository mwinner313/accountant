<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { useFactorStore } from '@/stores/factors'
import { FactorType, type FactorTypeValue } from '@/api/endpoints/factors'
import { toJalali } from '@/i18n/format'
import { ArrowDownLeft, ArrowUpRight, ChevronLeft } from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'

const { t } = useI18n()
const shops = useShopStore()
const factors = useFactorStore()
const router = useRouter()

const filterStr = ref('all')

const filter = computed<FactorTypeValue | null>(() => {
  if (filterStr.value === 'all') return null
  return Number(filterStr.value) as FactorTypeValue
})

async function reload() {
  if (!shops.activeShopId) return
  await factors.loadLocal(shops.activeShopId, filter.value)
  void factors.refresh(shops.activeShopId, filter.value)
}

onMounted(reload)
watch(() => shops.activeShopId, reload)
watch(filterStr, reload)

function open(id: string) {
  router.push({ name: 'factor-edit', params: { id } })
}

function newFactor(type: FactorTypeValue) {
  router.push({ name: 'factor-new', query: { type: String(type) } })
}
</script>

<template>
  <PageHeader :title="t('factors.title')">
    <template #actions>
      <Button size="sm" variant="outline" @click="newFactor(FactorType.Sell)">
        <ArrowUpRight class="size-4" />
        {{ t('factors.new_sell') }}
      </Button>
      <Button size="sm" variant="secondary" @click="newFactor(FactorType.Buy)">
        <ArrowDownLeft class="size-4" />
        {{ t('factors.new_buy') }}
      </Button>
    </template>
  </PageHeader>

  <ToggleGroup v-model="filterStr" type="single" variant="outline" class="filter-bar w-full">
    <ToggleGroupItem value="all" class="flex-1">{{ t('factors.filter_all') }}</ToggleGroupItem>
    <ToggleGroupItem :value="String(FactorType.Buy)" class="flex-1">{{
      t('factors.type_buy')
    }}</ToggleGroupItem>
    <ToggleGroupItem :value="String(FactorType.Sell)" class="flex-1">{{
      t('factors.type_sell')
    }}</ToggleGroupItem>
  </ToggleGroup>

  <Progress v-if="factors.loading" class="h-1 animate-pulse" :model-value="undefined" />

  <EmptyState
    v-if="!factors.loading && factors.items.length === 0"
    icon="receipt"
    :hint="t('factors.no_factors')"
  />

  <div v-else>
    <div
      v-for="f in factors.items"
      :key="f.factorId"
      class="list-card"
      @click="open(f.factorId)"
    >
      <div class="avatar" :class="{ buy: f.type === FactorType.Buy }">
        <ArrowDownLeft v-if="f.type === FactorType.Buy" class="size-5" />
        <ArrowUpRight v-else class="size-5" />
      </div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ f.type === FactorType.Buy ? t('factors.type_buy') : t('factors.type_sell') }}
          <Badge v-if="f.isReversed" variant="destructive">{{ t('factors.reversed') }}</Badge>
          <Badge v-if="f._local" variant="secondary">آفلاین</Badge>
        </div>
        <div class="list-card__sub">
          {{ toJalali(f.date) }} • {{ t('factors.item_count', { count: f.itemCount }) }}
          <template v-if="f.counterpartyFullName">
            <span class="dot">•</span>
            <span class="notes">{{ f.counterpartyFullName }}</span>
          </template>
          <span v-if="f.notes" class="dot">•</span>
          <span v-if="f.notes" class="notes">{{ f.notes }}</span>
        </div>
      </div>
      <ChevronLeft class="size-5 muted" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.filter-bar {
  margin-bottom: 1rem;
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
.dot {
  margin-inline: 0.25rem;
}
.notes {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 12ch;
  display: inline-block;
  vertical-align: bottom;
}
</style>
