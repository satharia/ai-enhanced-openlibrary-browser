import './assets/main.css'

import { createApp } from 'vue'
import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(PrimeVue, {
  theme: { preset: Aura },
  license: import.meta.env.VITE_PRIMEUI_LICENSE,
})
app.use(router)

app.mount('#app')
