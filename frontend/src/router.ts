import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
    {
        path: '/',
        component: () => import('./views/LoginView.vue')
    },
    {
        path: '/form',
        component: () => import('./views/student/FormView.vue')
    },
	{
        path: '/teacher',
        component: () => import('./views/teacher/TeacherDashboardView.vue'),
    },
    {
        path: '/teacher/solver',
        component: () => import('./views/teacher/SolverView.vue'),
    },
    {
        path: '/teacher/projects',
        component: () => import('./views/teacher/ProjectsView.vue')
    },
    {
        path: '/teacher/students',
        component: () => import('./views/teacher/StudentsView.vue'),
    }
]

export default createRouter({
    history: createWebHistory(),
    routes,
})
