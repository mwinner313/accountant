<script setup lang="ts">
import { computed, nextTick, onMounted, ref, useId, watch } from 'vue'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import JalaliDateTimePicker from 'vue3-persian-datetime-picker'
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
    /** When true and v-model is null on mount, emit today once. */
    defaultToday?: boolean
    /**
     * When `type` is `datetime` and this is false (default), the panel uses date-only
     * steps and keeps the clock from when the panel opened; confirm (تایید) applies.
     * Set true to use the library’s separate time step before confirm.
     */
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
/** Time-of-day preserved when `datetime` is edited as date-only (until user confirms). */
const preservedTimeAtOpen = ref<Date>(new Date())
const rawId = useId().replace(/[^a-zA-Z0-9_-]/g, '')
const inputId = `jpd-${rawId || '0'}`

const customInputSelector = computed(() => `#${inputId}`)

const hasValue = computed(() => props.modelValue != null && !Number.isNaN(props.modelValue.getTime()))

const withTime = computed(() => props.type === 'datetime' || props.type === 'time')

/** Library type: `datetime` without time step uses `date` so the panel stays on the calendar until تأیید. */
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

/** Keep the visible field in sync; vue3-persian-datetime-picker customInput sync is unreliable here. */
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
    <!-- PrimeVue-styled trigger; value text is written by vue3-persian-datetime-picker (customInput). -->
    <IconField :fluid="fluid" iconPosition="right" class="jpd-field">
      <InputIcon
        class="pi pi-calendar jpd-calendar-icon"
        :class="{ 'jpd-calendar-icon--disabled': disabled }"
        @click.stop="openFromIcon"
      />
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
        :class="[
          'p-inputtext p-component jpd-input',
          {
            'p-filled': hasValue,
            'p-invalid': invalid,
            'p-disabled': disabled
          },
          inputClass
        ]"
      />
    </IconField>

    <!-- Jalali engine: no built-in input; overlay only. -->
    <JalaliDateTimePicker
      class="jpd-picker-host"
      :model-value="modelValue ?? undefined"
      :type="libraryPickerType"
      locale="fa"
      :popover="true"
      :auto-submit="false"
      :wrapper-submit="false"
      :convert-numbers="true"
      color="var(--p-primary-color)"
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

  /* Lift entire field + panel above following form rows / PrimeVue layers (Select ~1000). */
  &--open {
    z-index: 2200;
  }
}

.jpd-field {
  width: 100%;
}

.jpd-input {
  width: 100%;
  min-width: 0;
  cursor: pointer;
}

.jpd-calendar-icon {
  cursor: pointer;
  &--disabled {
    cursor: not-allowed;
    opacity: 0.6;
  }
}

/*
 * Anchor .vpd-main below the trigger (zero height) so it does not sit on top of
 * the input — avoids pointer-events hacks that blocked clicks on the panel.
 */
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
