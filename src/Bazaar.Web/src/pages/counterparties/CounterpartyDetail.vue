<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useShopStore } from '@/stores/shops'
import { useFactorStore } from '@/stores/factors'
import { getCounterparty, type CounterpartyDetailModel } from '@/api/endpoints/counterparties'
import { FactorType } from '@/api/endpoints/factors'
import { toJalali } from '@/i18n/format'
import { DevBypassError } from '@/api/http'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
const shops = useShopStore()
const factors = useFactorStore()

const cpId = computed(() => route.params.id as string)
const detail = ref<CounterpartyDetailModel | null>(null)
const loadDetailError = ref<string | null>(null)

const cpFactors = computed(() =>
  factors.items.filter(f => f.counterpartyId != null && f.counterpartyId === cpId.value)
)

async function reload() {
  loadDetailError.value = null
  detail.value = null
  try {
    detail.value = await getCounterparty(cpId.value)
  } catch (e: any) {
    if (!(e instanceof DevBypassError)) loadDetailError.value = e?.message ?? t('app.error')
  }
  const shopId = shops.activeShopId
  if (shopId) {
    await factors.loadLocal(shopId, null)
    void factors.refresh(shopId, null)
  }
}

onMounted(reload)
watch(cpId, reload)
watch(() => shops.activeShopId, reload)

function openFactor(factorId: string) {
  router.push({ name: 'factor-edit', params: { id: factorId } })
}
</script>

<template>
  <PageHeader :title="detail?.fullName ?? t('factors.cp_detail')">
    <template #actions>
      <Button icon="pi pi-arrow-right" text rounded @click="router.back()" />
    </template>
  </PageHeader>

  <Message v-if="loadDetailError" severity="error" :closable="false" class="mb-3">
    {{ loadDetailError }}
  </Message>

  <template v-else-if="detail">
    <section v-if="detail.phones.length" class="block">
      <h3 class="sub">{{ t('counterparties.phones') }}</h3>
      <ul class="plain">
        <li v-for="(p, i) in detail.phones" :key="'ph' + i">{{ p.number }}</li>
      </ul>
    </section>

    <section v-if="detail.bankAccounts.length" class="block">
      <h3 class="sub">{{ t('counterparties.bank_accounts') }}</h3>
      <div v-for="(b, i) in detail.bankAccounts" :key="'bk' + i" class="bank-card">
        <div class="bank-line"><strong>{{ b.name }}</strong></div>
        <div v-if="b.accountNumber" class="bank-line muted">{{ b.accountNumber }}</div>
        <div v-if="b.shebaNumber" class="bank-line muted" dir="ltr">{{ b.shebaNumber }}</div>
        <div v-if="b.cardNumber" class="bank-line muted" dir="ltr">{{ b.cardNumber }}</div>
      </div>
    </section>

    <h3 class="section-title">{{ t('factors.cp_factors_title') }}</h3>
    <ProgressBar v-if="factors.loading" mode="indeterminate" style="height: 3px" />

    <EmptyState
      v-if="!factors.loading && cpFactors.length === 0"
      icon="pi-receipt"
      :hint="t('factors.cp_no_factors')"
    />

    <div v-else-if="cpFactors.length">
      <div
        v-for="f in cpFactors"
        :key="f.factorId"
        class="list-card"
        @click="openFactor(f.factorId)"
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
            <span v-if="f.notes" class="dot">•</span>
            <span v-if="f.notes" class="notes">{{ f.notes }}</span>
          </div>
        </div>
        <i class="pi pi-angle-left muted" />
      </div>
    </div>
  </template>
</template>

<style scoped lang="scss">
.mb-3 {
  margin-bottom: 1rem;
}
.block {
  margin-bottom: 1.25rem;
}
.sub {
  margin: 0 0 0.5rem;
  font-size: 0.95rem;
  font-weight: 700;
}
.plain {
  margin: 0;
  padding-inline-start: 1.25rem;
}
.bank-card {
  padding: 0.75rem;
  border-radius: 10px;
  border: 1px solid var(--p-content-border-color);
  margin-bottom: 0.5rem;
}
.bank-line {
  font-size: 0.9rem;
  &.muted {
    opacity: 0.85;
    font-size: 0.85rem;
  }
}
.section-title {
  margin: 1.25rem 0 0.75rem;
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
  background: var(--p-primary-100, #e0e7ff);
  color: var(--p-primary-color);
  &.buy {
    background: var(--p-green-100, #dcfce7);
    color: var(--p-green-700, #15803d);
  }
}
.dot {
  opacity: 0.5;
  margin: 0 0.2rem;
}
.notes {
  opacity: 0.85;
}
.muted {
  opacity: 0.5;
}
</style>
