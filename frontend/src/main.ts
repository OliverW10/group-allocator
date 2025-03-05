import { createApp } from 'vue'
import App from './App.vue'
import PrimeVue from 'primevue/config'
import Lara from '@primeuix/themes/lara'
import router from './router.ts'
import 'virtual:uno.css'
import '@unocss/reset/tailwind-compat.css'

const app = createApp(App)
app.use(PrimeVue, {
    theme: {
        preset: Lara,
    },
})
app.use(router)
app.mount('#app')
