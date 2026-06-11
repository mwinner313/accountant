import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'

import App from './App.vue'
import { router } from './router'
import { i18n } from './i18n'

import 'vazirmatn/Vazirmatn-Variable-font-face.css'
import './styles/app.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(i18n)
app.use(PrimeVue, {
  ripple: true,
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.app-dark',
      cssLayer: { name: 'primevue', order: 'reset,primevue,app' }
    }
  },
  locale: {
    accept: 'تایید',
    reject: 'انصراف',
    choose: 'انتخاب',
    upload: 'بارگذاری',
    cancel: 'انصراف',
    completed: 'تکمیل شد',
    pending: 'در انتظار',
    fileSizeTypes: ['ب', 'کیلوبایت', 'مگابایت', 'گیگابایت', 'ترابایت'],
    dayNames: ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنجشنبه', 'جمعه', 'شنبه'],
    dayNamesShort: ['یک', 'دو', 'سه', 'چهار', 'پنج', 'جمعه', 'شنبه'],
    dayNamesMin: ['ی', 'د', 'س', 'چ', 'پ', 'ج', 'ش'],
    monthNames: [
      'ژانویه',
      'فوریه',
      'مارس',
      'آوریل',
      'مه',
      'ژوئن',
      'ژوئیه',
      'اوت',
      'سپتامبر',
      'اکتبر',
      'نوامبر',
      'دسامبر'
    ],
    monthNamesShort: [
      'ژانویه',
      'فوریه',
      'مارس',
      'آوریل',
      'مه',
      'ژوئن',
      'ژوئیه',
      'اوت',
      'سپتامبر',
      'اکتبر',
      'نوامبر',
      'دسامبر'
    ],
    today: 'امروز',
    clear: 'پاک کردن',
    weekHeader: 'هفته',
    firstDayOfWeek: 6,
    dateFormat: 'yy/mm/dd',
    weak: 'ضعیف',
    medium: 'متوسط',
    strong: 'قوی',
    passwordPrompt: 'یک رمز عبور وارد کنید',
    emptyFilterMessage: 'موردی یافت نشد',
    emptyMessage: 'موردی برای نمایش نیست'
  }
})
app.use(ToastService)
app.use(ConfirmationService)

app.mount('#app')
