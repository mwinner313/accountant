import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import { router } from './router'
import { i18n } from './i18n'

import 'vazirmatn/Vazirmatn-Variable-font-face.css'
import './styles/tailwind.css'
import './styles/app.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(i18n)

app.mount('#app')
