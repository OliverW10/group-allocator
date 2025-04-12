import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
    {
        path: '/',
        component: () => import('./views/LoginView.vue')
    },
    {
        path: '/form',
        component: () => import('./views/FormView.vue')
    },
    {
        path: '/admin/solver',
        component: () => import('./views/admin/SolverView.vue'),
    },
    {
        path: '/admin/projects',
        component: () => import('./views/admin/ProjectsView.vue')
    },
    {
        path: '/admin/students',
        component: () => import('./views/admin/StudentsView.vue'),
        props: true,
    }
]

export default createRouter({
    history: createWebHistory(),
    routes,
})
