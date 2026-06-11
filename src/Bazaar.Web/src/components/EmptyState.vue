<script setup lang="ts">
defineProps<{
  icon?: string
  title?: string
  hint?: string
}>()
</script>

<template>
  <div class="empty-state">
    <i :class="['pi', icon ?? 'pi-inbox']" />
    <div v-if="title" class="title">{{ title }}</div>
    <div v-if="hint" class="hint">{{ hint }}</div>
    <slot />
  </div>
</template>

<style scoped lang="scss">
.title {
  font-weight: 600;
  font-size: 1rem;
  color: var(--p-text-color);
  margin-bottom: 0.25rem;
}
.hint {
  font-size: 0.9rem;
}
</style>
