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
        component: () => import('./views/StudentPreferencesView.vue')
    },
    {
        path: '/projects',
        component: () => import('./views/Projects.vue')
    },
    {
        path: '/project/:projectId',
        component: () => import('./views/ProjectDetailsView.vue'),
        props: true,
    }
]

export default createRouter({
    history: createWebHistory(),
    routes,
})
