<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { useProductStore } from '@/stores/products'
import { useCategoryStore } from '@/stores/categories'
import { useProductPropertyStore } from '@/stores/productProperties'
import { toast } from 'vue-sonner'
import type { CreateProductPayload } from '@/api/endpoints/products'
import { ArrowRight, Check } from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select'

defineProps<{ id?: string }>()

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const shops = useShopStore()
const products = useProductStore()
const cats = useCategoryStore()
const props = useProductPropertyStore()

const id = computed(() => (route.params.id as string | undefined) ?? null)
const isNew = computed(() => !id.value)

const form = ref<CreateProductPayload>({
  categoryId: null,
  name: '',
  unit: '',
  picture: null,
  sellPrice: 0,
  buyPrice: 0
})

const propEdits = ref<Record<string, string>>({})

const categorySelect = computed({
  get: () => (form.value.categoryId == null ? 'none' : String(form.value.categoryId)),
  set: (v: string) => {
    form.value.categoryId = v === 'none' ? null : v
  }
})

const pictureInput = computed({
  get: () => form.value.picture ?? '',
  set: (v: string) => {
    form.value.picture = v || null
  }
})

async function reload() {
  if (!shops.activeShopId) return
  void cats.refresh(shops.activeShopId)
  void props.refresh(shops.activeShopId)
  if (id.value) {
    await products.loadDetail(shops.activeShopId, id.value)
    const d = products.detail
    if (d) {
      form.value = {
        categoryId: d.categoryId ?? null,
        name: d.name,
        unit: d.unit,
        picture: d.picture ?? null,
        sellPrice: Number(d.sellPrice),
        buyPrice: Number(d.buyPrice)
      }
      for (const pv of d.properties ?? []) propEdits.value[pv.propertyId] = pv.value
    }
  }
}

onMounted(reload)
watch(() => route.params.id, reload)

async function save() {
  if (!shops.activeShopId) return
  if (!form.value.name.trim() || !form.value.unit.trim()) {
    toast.warning(t('validation.required'))
    return
  }
  if (isNew.value) {
    const created = await products.create(shops.activeShopId, { ...form.value })
    toast.success(t('app.saved_locally'))
    router.replace({ name: 'product-edit', params: { id: created.productId } })
  } else if (id.value) {
    await products.update(shops.activeShopId, id.value, { ...form.value })
    toast.success(t('app.saved_locally'))
  }
}

async function saveProperty(propertyId: string) {
  if (!shops.activeShopId || !id.value) return
  const value = propEdits.value[propertyId] ?? ''
  await products.setProperty(shops.activeShopId, id.value, propertyId, value)
  toast.success(t('app.saved_locally'))
}
</script>

<template>
  <PageHeader :title="isNew ? t('products.new') : t('app.edit')">
    <template #actions>
      <Button variant="ghost" size="icon" @click="router.back()">
        <ArrowRight class="size-4" />
      </Button>
    </template>
  </PageHeader>

  <form class="form" @submit.prevent="save">
    <div class="field">
      <Label for="product-name">{{ t('products.name_label') }}</Label>
      <Input id="product-name" v-model="form.name" class="mt-1" />
    </div>

    <div class="row">
      <div class="field">
        <Label for="product-unit">{{ t('products.unit_label') }}</Label>
        <Input
          id="product-unit"
          v-model="form.unit"
          :placeholder="t('products.unit_placeholder')"
          class="mt-1"
        />
      </div>
      <div class="field">
        <Label>{{ t('products.category') }}</Label>
        <Select v-model="categorySelect" class="mt-1">
          <SelectTrigger class="w-full">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="none">{{ t('products.no_category') }}</SelectItem>
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
    </div>

    <div class="row">
      <div class="field">
        <Label for="sell-price">{{ t('products.sell_price') }}</Label>
        <Input
          id="sell-price"
          type="number"
          min="0"
          :model-value="String(form.sellPrice)"
          class="mt-1"
          @update:model-value="v => (form.sellPrice = Number(v) || 0)"
        />
      </div>
      <div class="field">
        <Label for="buy-price">{{ t('products.buy_price') }}</Label>
        <Input
          id="buy-price"
          type="number"
          min="0"
          :model-value="String(form.buyPrice)"
          class="mt-1"
          @update:model-value="v => (form.buyPrice = Number(v) || 0)"
        />
      </div>
    </div>

    <div class="field">
      <Label for="product-picture">{{ t('products.picture') }}</Label>
      <Input id="product-picture" v-model="pictureInput" dir="ltr" class="mt-1" />
    </div>

    <Button type="submit" class="save-btn w-full">
      <Check class="size-4" />
      {{ t('app.save') }}
    </Button>
  </form>

  <section v-if="!isNew && props.items.length" class="properties">
    <h3 class="section-title">{{ t('products.properties') }}</h3>
    <div v-for="p in props.items" :key="p.productPropertyId" class="prop-row">
      <div class="prop-name">{{ p.name }}</div>
      <Input
        v-model="propEdits[p.productPropertyId]"
        :placeholder="t('products.property_value')"
      />
      <Button
        size="sm"
        :aria-label="t('products.save_property')"
        @click="saveProperty(p.productPropertyId)"
      >
        <Check class="size-4" />
      </Button>
    </div>
  </section>
</template>

<style scoped lang="scss">
.form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}
.field {
  display: block;
}
.save-btn {
  margin-top: 0.5rem;
}
.section-title {
  margin: 1.5rem 0 0.75rem;
  font-size: 1rem;
  font-weight: 700;
}
.prop-row {
  display: grid;
  grid-template-columns: 120px 1fr auto;
  gap: 0.5rem;
  align-items: center;
  margin-bottom: 0.5rem;
}
.prop-name {
  font-weight: 600;
  font-size: 0.9rem;
}
</style>
