import { onMounted, onUnmounted, ref } from 'vue'
import { pendingCount } from '@/db/outbox'

export function usePendingSync(intervalMs = 5000) {
  const count = ref(0)
  let timer: number | null = null

  async function refresh() {
    try {
      count.value = await pendingCount()
    } catch {
      // ignore
    }
  }

  onMounted(() => {
    void refresh()
    timer = window.setInterval(refresh, intervalMs)
  })

  onUnmounted(() => {
    if (timer !== null) window.clearInterval(timer)
  })

  return { count, refresh }
}
