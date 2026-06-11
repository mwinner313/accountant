<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { useProductStore } from '@/stores/products'
import { useCategoryStore } from '@/stores/categories'
import { useConfirm } from 'primevue/useconfirm'
import { formatDecimal, formatMoney } from '@/i18n/format'
import { refDebounced } from '@vueuse/core'

const { t } = useI18n()
const shops = useShopStore()
const products = useProductStore()
const cats = useCategoryStore()
const router = useRouter()
const confirm = useConfirm()

const search = ref('')
const debouncedSearch = refDebounced(search, 300)
const selectedCat = ref<string | null>(null)

const categoryOptions = computed(() => [
  { categoryId: null, name: t('app.all') },
  ...cats.items.map(c => ({ categoryId: c.categoryId, name: c.name }))
])

const filterParams = computed(() => ({
  categoryId: selectedCat.value,
  search: debouncedSearch.value || undefined
}))

async function reload() {
  if (!shops.activeShopId) return
  await Promise.all([
    cats.loadLocal(shops.activeShopId),
    products.loadLocal(shops.activeShopId, filterParams.value)
  ])
  void cats.refresh(shops.activeShopId)
  void products.refresh(shops.activeShopId, filterParams.value)
}

onMounted(reload)
watch(() => shops.activeShopId, reload)
watch(filterParams, async () => {
  if (!shops.activeShopId) return
  await products.loadLocal(shops.activeShopId, filterParams.value)
  void products.refresh(shops.activeShopId, filterParams.value)
})

function open(id: string) {
  router.push({ name: 'product-edit', params: { id } })
}
function openNew() {
  router.push({ name: 'product-new' })
}

function askDelete(id: string) {
  confirm.require({
    message: t('products.delete_confirm'),
    acceptClass: 'p-button-danger',
    accept: async () => {
      if (shops.activeShopId) await products.remove(shops.activeShopId, id)
    }
  })
}

function catName(id?: string | null): string {
  if (!id) return t('products.no_category')
  return cats.items.find(c => c.categoryId === id)?.name ?? '—'
}
</script>

<template>
  <PageHeader :title="t('products.title')">
    <template #actions>
      <Button :label="t('products.new')" icon="pi pi-plus" size="small" @click="openNew" />
    </template>
  </PageHeader>

  <div class="filters">
    <IconField iconPosition="right" fluid>
      <InputIcon class="pi pi-search" />
      <InputText v-model="search" :placeholder="t('products.search_placeholder')" fluid />
    </IconField>
    <Select
      v-model="selectedCat"
      :options="categoryOptions"
      optionLabel="name"
      optionValue="categoryId"
      :placeholder="t('products.category')"
      class="cat-select"
    />
  </div>

  <ProgressBar v-if="products.loading" mode="indeterminate" style="height: 3px" />

  <EmptyState
    v-if="!products.loading && products.items.length === 0"
    icon="pi-box"
    :hint="t('products.no_products')"
  >
    <Button :label="t('products.new')" icon="pi pi-plus" @click="openNew" class="mt-3" />
  </EmptyState>

  <div v-else>
    <div
      v-for="p in products.items"
      :key="p.productId"
      class="list-card product-card"
      @click="open(p.productId)"
    >
      <div class="thumb">
        <img v-if="p.picture" :src="p.picture" :alt="p.name" />
        <i v-else class="pi pi-box" />
      </div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ p.name }}
          <Tag v-if="p._local" value="آفلاین" severity="info" />
        </div>
        <div class="list-card__sub">
          {{ catName(p.categoryId) }} • {{ t('products.inventory') }}:
          {{ formatDecimal(p.inventoryAmount) }} {{ p.unit }}
        </div>
      </div>
      <div class="prices">
        <div class="list-card__price">{{ formatMoney(p.sellPrice) }}</div>
        <Button
          icon="pi pi-trash"
          text
          rounded
          size="small"
          severity="danger"
          @click.stop="askDelete(p.productId)"
        />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.filters {
  display: grid;
  grid-template-columns: 1fr 160px;
  gap: 0.5rem;
  margin-bottom: 1rem;

  .cat-select {
    min-width: 0;
  }
}
.product-card {
  align-items: stretch;
}
.thumb {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  background: var(--p-surface-100, #f1f5f9);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  flex-shrink: 0;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
  i {
    font-size: 1.25rem;
    color: var(--p-text-muted-color);
  }
}
.prices {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
}
.mt-3 {
  margin-top: 0.75rem;
}
</style>
