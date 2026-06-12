<script setup lang="ts">
import { onMounted } from 'vue'
import { useAuthStore } from '@/auth/auth.store'
import { startBackgroundSync } from '@/sync/syncEngine'
import { Toaster } from '@/components/ui/sonner'
import ConfirmHost from '@/components/ConfirmHost.vue'

const auth = useAuthStore()

onMounted(async () => {
  await auth.hydrate()
  startBackgroundSync()
})
</script>

<template>
  <Toaster position="top-center" :dir="'rtl'" />
  <ConfirmHost />
  <router-view v-slot="{ Component }">
    <transition name="fade" mode="out-in">
      <component :is="Component" />
    </transition>
  </router-view>
</template>

<style>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
