import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
    {
        path: '/',
        component: () => import('./views/Home.vue'),
    },
    {
        path: '/account',
        component: () => import('./views/Account.vue')
    }
]

export default createRouter({
    history: createWebHistory(),
    routes,
})
