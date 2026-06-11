<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { useFactorStore } from '@/stores/factors'
import { FactorType, type FactorTypeValue } from '@/api/endpoints/factors'
import { toJalali } from '@/i18n/format'

const { t } = useI18n()
const shops = useShopStore()
const factors = useFactorStore()
const router = useRouter()

const filter = ref<FactorTypeValue | null>(null)

async function reload() {
  if (!shops.activeShopId) return
  await factors.loadLocal(shops.activeShopId, filter.value)
  void factors.refresh(shops.activeShopId, filter.value)
}

onMounted(reload)
watch(() => shops.activeShopId, reload)
watch(filter, reload)

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
      <Button
        :label="t('factors.new_sell')"
        icon="pi pi-arrow-up-right"
        size="small"
        severity="warning"
        @click="newFactor(FactorType.Sell)"
      />
      <Button
        :label="t('factors.new_buy')"
        icon="pi pi-arrow-down-left"
        size="small"
        severity="success"
        @click="newFactor(FactorType.Buy)"
      />
    </template>
  </PageHeader>

  <SelectButton
    v-model="filter"
    :options="[
      { value: null, label: t('factors.filter_all') },
      { value: FactorType.Buy, label: t('factors.type_buy') },
      { value: FactorType.Sell, label: t('factors.type_sell') }
    ]"
    optionLabel="label"
    optionValue="value"
    class="filter-bar"
  />

  <ProgressBar v-if="factors.loading" mode="indeterminate" style="height: 3px" />

  <EmptyState
    v-if="!factors.loading && factors.items.length === 0"
    icon="pi-receipt"
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
        <i
          :class="['pi', f.type === FactorType.Buy ? 'pi-arrow-down-left' : 'pi-arrow-up-right']"
        />
      </div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ f.type === FactorType.Buy ? t('factors.type_buy') : t('factors.type_sell') }}
          <Tag v-if="f.isReversed" :value="t('factors.reversed')" severity="danger" />
          <Tag v-if="f._local" value="آفلاین" severity="info" />
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
      <i class="pi pi-angle-left muted" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.filter-bar {
  margin-bottom: 1rem;
  display: flex;
  gap: 0.25rem;
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
