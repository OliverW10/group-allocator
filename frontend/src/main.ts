import { createApp } from 'vue'
import App from './App.vue'
import PrimeVue from 'primevue/config'
import Lara from '@primeuix/themes/lara'
import router from './router.ts'
import { createPinia } from 'pinia'
import 'virtual:uno.css'
import '@unocss/reset/tailwind-compat.css'

const app = createApp(App)
const pinia = createPinia()

app.use(PrimeVue, {
    theme: {
        preset: Lara,
    },
})
app.use(pinia)
app.use(router)

app.mount('#app')
