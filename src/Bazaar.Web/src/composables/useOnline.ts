import { onMounted, onUnmounted, ref } from 'vue'

export function useOnline() {
  const online = ref(typeof navigator === 'undefined' ? true : navigator.onLine)

  const setOnline = () => {
    online.value = true
  }
  const setOffline = () => {
    online.value = false
  }

  onMounted(() => {
    window.addEventListener('online', setOnline)
    window.addEventListener('offline', setOffline)
  })
  onUnmounted(() => {
    window.removeEventListener('online', setOnline)
    window.removeEventListener('offline', setOffline)
  })

  return { online }
}
