<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { useProductStore } from '@/stores/products'
import { useCategoryStore } from '@/stores/categories'
import { useConfirm } from '@/composables/useConfirm'
import { formatDecimal, formatMoney } from '@/i18n/format'
import { refDebounced } from '@vueuse/core'
import { Box, Plus, Search, Trash2 } from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select'

const { t } = useI18n()
const shops = useShopStore()
const products = useProductStore()
const cats = useCategoryStore()
const router = useRouter()
const confirm = useConfirm()

const search = ref('')
const debouncedSearch = refDebounced(search, 300)
const selectedCat = ref<string>('none')

const filterParams = computed(() => ({
  categoryId: selectedCat.value === 'none' ? null : selectedCat.value,
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
      <Button size="sm" @click="openNew">
        <Plus class="size-4" />
        {{ t('products.new') }}
      </Button>
    </template>
  </PageHeader>

  <div class="filters">
    <div class="search-wrap">
      <Search class="search-icon size-4" />
      <Input v-model="search" :placeholder="t('products.search_placeholder')" class="search-input" />
    </div>
    <Select v-model="selectedCat">
      <SelectTrigger class="cat-select">
        <SelectValue :placeholder="t('products.category')" />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value="none">{{ t('app.all') }}</SelectItem>
        <SelectItem
          v-for="c in cats.items"
          :key="c.categoryId"
          :value="String(c.categoryId)"
        >
          {{ c.name }}
        </SelectItem>
      </SelectContent>
    </Select>
  </div>

  <Progress v-if="products.loading" class="h-1 animate-pulse" :model-value="undefined" />

  <EmptyState
    v-if="!products.loading && products.items.length === 0"
    icon="box"
    :hint="t('products.no_products')"
  >
    <Button class="mt-3" @click="openNew">
      <Plus class="size-4" />
      {{ t('products.new') }}
    </Button>
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
        <Box v-else class="size-6 text-muted-foreground" />
      </div>
      <div class="list-card__body">
        <div class="list-card__title">
          {{ p.name }}
          <Badge v-if="p._local" variant="secondary">آفلاین</Badge>
        </div>
        <div class="list-card__sub">
          {{ catName(p.categoryId) }} • {{ t('products.inventory') }}:
          {{ formatDecimal(p.inventoryAmount) }} {{ p.unit }}
        </div>
      </div>
      <div class="prices">
        <div class="list-card__price">{{ formatMoney(p.sellPrice) }}</div>
        <Button
          variant="ghost"
          size="icon"
          class="size-8"
          @click.stop="askDelete(p.productId)"
        >
          <Trash2 class="size-4 text-destructive" />
        </Button>
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
.search-wrap {
  position: relative;

  .search-icon {
    position: absolute;
    inset-inline-end: 0.75rem;
    top: 50%;
    transform: translateY(-50%);
    color: var(--muted-foreground);
    pointer-events: none;
  }
  .search-input {
    padding-inline-end: 2.5rem;
  }
}
.product-card {
  align-items: stretch;
}
.thumb {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  background: var(--muted);
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
