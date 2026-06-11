<script setup lang="ts">
import { computed, nextTick, onMounted, ref, useId, watch } from 'vue'
import { Calendar } from '@lucide/vue'
import JalaliDateTimePicker from 'vue3-persian-datetime-picker'
import { Input } from '@/components/ui/input'
import { fromJalaliInput, toJalali } from '@/i18n/format'

export type JalaliPickerType = 'date' | 'datetime' | 'time' | 'year' | 'month' | 'year-month'

const props = withDefaults(
  defineProps<{
    modelValue: Date | null
    type?: JalaliPickerType
    disabled?: boolean
    placeholder?: string
    fluid?: boolean
    invalid?: boolean
    min?: string
    max?: string
    inputClass?: string | string[] | Record<string, boolean>
    defaultToday?: boolean
    selectTimeInPanel?: boolean
  }>(),
  {
    type: 'datetime',
    disabled: false,
    fluid: false,
    placeholder: '',
    invalid: false,
    min: undefined,
    max: undefined,
    inputClass: '',
    defaultToday: true,
    selectTimeInPanel: false
  }
)

const emit = defineEmits<{
  'update:modelValue': [value: Date | null]
}>()

const proxyInputRef = ref<HTMLInputElement | null>(null)
const pickerOpen = ref(false)
const preservedTimeAtOpen = ref<Date>(new Date())
const rawId = useId().replace(/[^a-zA-Z0-9_-]/g, '')
const inputId = `jpd-${rawId || '0'}`

const customInputSelector = computed(() => `#${inputId}`)

const hasValue = computed(() => props.modelValue != null && !Number.isNaN(props.modelValue.getTime()))

const withTime = computed(() => props.type === 'datetime' || props.type === 'time')

const libraryPickerType = computed<JalaliPickerType>(() => {
  if (props.type === 'datetime' && !props.selectTimeInPanel) return 'date'
  return props.type
})

function mergeDateKeepingTime(dateOnly: Date, timeFrom: Date): Date {
  const out = new Date(dateOnly.getTime())
  out.setHours(
    timeFrom.getHours(),
    timeFrom.getMinutes(),
    timeFrom.getSeconds(),
    timeFrom.getMilliseconds()
  )
  return out
}

function onPickerOpen() {
  pickerOpen.value = true
  const m = props.modelValue
  preservedTimeAtOpen.value =
    m != null && !Number.isNaN(m.getTime()) ? new Date(m.getTime()) : new Date()
}

function onPickerClose() {
  pickerOpen.value = false
}

function syncInputFromModel() {
  nextTick(() => {
    const el = proxyInputRef.value
    if (!el) return
    const d = props.modelValue
    if (d == null || Number.isNaN(d.getTime())) {
      el.value = ''
      return
    }
    el.value = toJalali(d, withTime.value)
  })
}

watch(
  () => (props.modelValue == null ? null : props.modelValue.getTime()),
  () => syncInputFromModel(),
  { immediate: true }
)

onMounted(() => {
  if (props.defaultToday && props.modelValue == null) {
    emit('update:modelValue', new Date())
  }
  syncInputFromModel()
})

function emitFromPickerDate(d: Date) {
  if (props.type === 'datetime' && !props.selectTimeInPanel) {
    emit('update:modelValue', mergeDateKeepingTime(d, preservedTimeAtOpen.value))
    return
  }
  emit('update:modelValue', d)
}

function onPickerUpdate(v: Date | string | Date[] | null | undefined) {
  if (v instanceof Date) {
    emitFromPickerDate(v)
    return
  }
  if (Array.isArray(v) && v[0] instanceof Date) {
    emitFromPickerDate(v[0])
    return
  }
  if (v === null || v === undefined || v === '') {
    emit('update:modelValue', null)
    return
  }
  if (typeof v === 'string') {
    const parsed = fromJalaliInput(v)
    if (!parsed) return
    if (props.type === 'datetime' && !props.selectTimeInPanel) {
      emit('update:modelValue', mergeDateKeepingTime(parsed, preservedTimeAtOpen.value))
    } else {
      emit('update:modelValue', parsed)
    }
  }
}

function openFromIcon() {
  if (props.disabled) return
  proxyInputRef.value?.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }))
}
</script>

<template>
  <div class="jpd-root" :class="{ 'jpd-root--fluid': fluid, 'jpd-root--open': pickerOpen }">
    <div class="jpd-field" :class="{ 'jpd-field--fluid': fluid }">
      <div class="jpd-input-wrap">
        <input
          :id="inputId"
          ref="proxyInputRef"
          type="text"
          readonly
          autocomplete="off"
          role="combobox"
          aria-haspopup="dialog"
          :placeholder="placeholder"
          :disabled="disabled"
          :aria-invalid="invalid || undefined"
          :class="['jpd-input', inputClass]"
        />
        <button
          type="button"
          class="jpd-calendar-btn"
          :class="{ 'jpd-calendar-btn--disabled': disabled }"
          :disabled="disabled"
          tabindex="-1"
          @click.stop="openFromIcon"
        >
          <Calendar class="size-4" />
        </button>
      </div>
    </div>

    <JalaliDateTimePicker
      class="jpd-picker-host"
      :model-value="modelValue ?? undefined"
      :type="libraryPickerType"
      locale="fa"
      :popover="true"
      :auto-submit="false"
      :wrapper-submit="false"
      :convert-numbers="true"
      color="var(--primary)"
      :custom-input="customInputSelector"
      :disabled="disabled"
      :placeholder="placeholder"
      :min="min"
      :max="max"
      @update:model-value="onPickerUpdate"
      @open="onPickerOpen"
      @close="onPickerClose"
    />
  </div>
</template>

<style scoped lang="scss">
.jpd-root {
  position: relative;
  display: inline-block;
  max-width: 100%;
  vertical-align: middle;

  &--fluid {
    display: block;
    width: 100%;
  }

  &--open {
    z-index: 2200;
  }
}

.jpd-field {
  width: 100%;

  &--fluid {
    display: block;
    width: 100%;
  }
}

.jpd-input-wrap {
  position: relative;
  width: 100%;
}

.jpd-input {
  width: 100%;
  min-width: 0;
  cursor: pointer;
  height: 2.25rem;
  border-radius: 0.375rem;
  border: 1px solid var(--input);
  background: transparent;
  padding-inline: 0.75rem 2.5rem;
  font-size: 0.875rem;
  outline: none;

  &:focus-visible {
    border-color: var(--ring);
    box-shadow: 0 0 0 3px color-mix(in oklch, var(--ring) 50%, transparent);
  }

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  &[aria-invalid='true'] {
    border-color: var(--destructive);
  }
}

.jpd-calendar-btn {
  position: absolute;
  inset-inline-end: 0.5rem;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  color: var(--muted-foreground);
  cursor: pointer;
  padding: 0.25rem;

  &--disabled {
    cursor: not-allowed;
    opacity: 0.6;
  }
}

.jpd-picker-host {
  position: absolute;
  left: 0;
  right: 0;
  top: 100%;
  width: 100%;
  height: 0;
  margin: 0;
  padding: 0;
  overflow: visible;
}
</style>
