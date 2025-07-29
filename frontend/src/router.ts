import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
    {
        path: '/',
        component: () => import('./views/LoginView.vue')
    },
    {
        path: '/form/:classId',
        component: () => import('./views/student/FormView.vue')
    },
    {
        path: '/class-select',
        component: () => import('./views/student/ClassSelect.vue')
    },
	{
        path: '/teacher',
        component: () => import('./views/teacher/TeacherDashboardView.vue'),
    },
    {
        path: '/teacher/:classId/solver',
        component: () => import('./views/teacher/SolverView.vue'),
    },
    {
        path: '/teacher/:classId/projects',
        component: () => import('./views/teacher/ProjectsView.vue')
    },
    {
        path: '/teacher/:classId/students',
        component: () => import('./views/teacher/StudentsView.vue'),
    },
    {
        path: '/teacher/:classId/purchase',
        component: () => import('./views/teacher/Purchased.vue'),
    },
    {
        path: '/teacher/:classId/code',
        component: () => import('./views/teacher/CodeView.vue'),
    },
    {
        path: '/teacher/:classId/teachers',
        component: () => import('./views/teacher/TeachersAdminPage.vue'),
    },
    
]

export default createRouter({
    history: createWebHistory(),
    routes,
})
