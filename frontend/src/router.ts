import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
    {
        path: '/',
        component: () => import('./views/LoginView.vue')
    },
    {
        path: '/dashboard',
        component: () => import('./views/AdminDashboardView.vue'),
    },
    {
        path: '/form',
        component: () => import('./views/StudentFormView.vue'),
    },
    {
        path: '/student/preferences',
        component: () => import('./views/StudentPreferencesView.vue')
    },
]

export default createRouter({
    history: createWebHistory(),
    routes,
})
