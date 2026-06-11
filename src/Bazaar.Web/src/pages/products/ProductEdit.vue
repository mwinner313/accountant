<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useShopStore } from '@/stores/shops'
import { useProductStore } from '@/stores/products'
import { useCategoryStore } from '@/stores/categories'
import { useProductPropertyStore } from '@/stores/productProperties'
import { useToast } from 'primevue/usetoast'
import type { CreateProductPayload } from '@/api/endpoints/products'

defineProps<{ id?: string }>()

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const toast = useToast()
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
    toast.add({ severity: 'warn', summary: t('validation.required'), life: 2000 })
    return
  }
  if (isNew.value) {
    const created = await products.create(shops.activeShopId, { ...form.value })
    toast.add({ severity: 'success', summary: t('app.saved_locally'), life: 2000 })
    router.replace({ name: 'product-edit', params: { id: created.productId } })
  } else if (id.value) {
    await products.update(shops.activeShopId, id.value, { ...form.value })
    toast.add({ severity: 'success', summary: t('app.saved_locally'), life: 2000 })
  }
}

async function saveProperty(propertyId: string) {
  if (!shops.activeShopId || !id.value) return
  const value = propEdits.value[propertyId] ?? ''
  await products.setProperty(shops.activeShopId, id.value, propertyId, value)
  toast.add({ severity: 'success', summary: t('app.saved_locally'), life: 1500 })
}
</script>

<template>
  <PageHeader :title="isNew ? t('products.new') : t('app.edit')">
    <template #actions>
      <Button icon="pi pi-arrow-right" text rounded @click="router.back()" />
    </template>
  </PageHeader>

  <form class="form" @submit.prevent="save">
    <label class="field">
      <span class="label">{{ t('products.name_label') }}</span>
      <InputText v-model="form.name" fluid />
    </label>

    <div class="row">
      <label class="field">
        <span class="label">{{ t('products.unit_label') }}</span>
        <InputText v-model="form.unit" :placeholder="t('products.unit_placeholder')" fluid />
      </label>
      <label class="field">
        <span class="label">{{ t('products.category') }}</span>
        <Select
          v-model="form.categoryId"
          :options="[{ categoryId: null, name: t('products.no_category') }, ...cats.items]"
          optionLabel="name"
          optionValue="categoryId"
          fluid
        />
      </label>
    </div>

    <div class="row">
      <label class="field">
        <span class="label">{{ t('products.sell_price') }}</span>
        <InputNumber v-model="form.sellPrice" :min="0" :minFractionDigits="0" fluid />
      </label>
      <label class="field">
        <span class="label">{{ t('products.buy_price') }}</span>
        <InputNumber v-model="form.buyPrice" :min="0" :minFractionDigits="0" fluid />
      </label>
    </div>

    <label class="field">
      <span class="label">{{ t('products.picture') }}</span>
      <InputText v-model="form.picture" dir="ltr" fluid />
    </label>

    <Button type="submit" :label="t('app.save')" icon="pi pi-check" fluid class="save-btn" />
  </form>

  <section v-if="!isNew && props.items.length" class="properties">
    <h3 class="section-title">{{ t('products.properties') }}</h3>
    <div v-for="p in props.items" :key="p.productPropertyId" class="prop-row">
      <div class="prop-name">{{ p.name }}</div>
      <InputText
        v-model="propEdits[p.productPropertyId]"
        :placeholder="t('products.property_value')"
        fluid
      />
      <Button
        icon="pi pi-check"
        size="small"
        @click="saveProperty(p.productPropertyId)"
        :aria-label="t('products.save_property')"
      />
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
  .label {
    display: block;
    font-weight: 600;
    font-size: 0.9rem;
    margin-bottom: 0.4rem;
  }
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
