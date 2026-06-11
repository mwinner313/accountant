<script setup lang="ts">
import { computed, type Component } from 'vue'
import { Box, Inbox, Receipt, SlidersHorizontal, Store, Tags, Users } from '@lucide/vue'

const props = defineProps<{
  icon?: string | Component
  title?: string
  hint?: string
}>()

const iconMap: Record<string, Component> = {
  'pi-shop': Store,
  'pi-box': Box,
  'pi-receipt': Receipt,
  'pi-tags': Tags,
  'pi-users': Users,
  'pi-sliders-h': SlidersHorizontal,
  'pi-inbox': Inbox,
  store: Store,
  box: Box,
  receipt: Receipt,
  tags: Tags,
  users: Users,
  sliders: SlidersHorizontal,
  inbox: Inbox
}

const resolvedIcon = computed<Component>(() => {
  if (props.icon && typeof props.icon !== 'string') return props.icon
  if (typeof props.icon === 'string') return iconMap[props.icon] ?? Inbox
  return Inbox
})
</script>

<template>
  <div class="empty-state">
    <component :is="resolvedIcon" />
    <div v-if="title" class="title">{{ title }}</div>
    <div v-if="hint" class="hint">{{ hint }}</div>
    <slot />
  </div>
</template>

<style scoped lang="scss">
.title {
  font-weight: 600;
  font-size: 1rem;
  color: var(--foreground);
  margin-bottom: 0.25rem;
}
.hint {
  font-size: 0.9rem;
}
</style>
