import { ref } from 'vue'

export interface ConfirmOptions {
  message: string
  accept?: () => void | Promise<void>
  reject?: () => void
}

const open = ref(false)
const options = ref<ConfirmOptions | null>(null)

export function useConfirm() {
  function require(opts: ConfirmOptions) {
    options.value = opts
    open.value = true
  }

  async function accept() {
    const cb = options.value?.accept
    open.value = false
    options.value = null
    if (cb) await cb()
  }

  function cancel() {
    options.value?.reject?.()
    open.value = false
    options.value = null
  }

  return { require, open, options, accept, cancel }
}
