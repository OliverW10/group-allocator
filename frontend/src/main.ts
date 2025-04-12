import { createApp } from 'vue'
import App from './App.vue'
import PrimeVue from 'primevue/config'
import Lara from '@primeuix/themes/lara'
import router from './router.ts'
import { createPinia } from 'pinia'
import 'virtual:uno.css'
import '@unocss/reset/tailwind-compat.css'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import ToastService from 'primevue/toastservice';

const app = createApp(App)
const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

app.use(PrimeVue, {
    theme: {
        preset: Lara,
    },
})
app.use(pinia)
app.use(router)
app.use(ToastService);

app.mount('#app')
