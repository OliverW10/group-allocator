import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
    {
        path: '/form',
        component: () => import('./views/StudentForm.vue'),
    },
    {
        path: '/',
        component: () => import('./views/Login.vue')
    },
    {
        path: '/dashboard',
        component: () => import('./views/AdminDashboard.vue'),
    },
]

export default createRouter({
    history: createWebHistory(),
    routes,
})
