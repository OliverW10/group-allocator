<template>
    <nav class="navbar">
        <div class="container">
            <a href="/" class="logo">MyApp</a>
            <Menu :model="menuItems" />
        </div>
    </nav>
</template>

<script setup lang="ts">
import Menu from 'primevue/menu';

const menuItems = [
    { label: 'Dashboard', icon: 'pi pi-info', command: () => window.location.href = '/dashboard' },
    { label: 'Projects', icon: 'pi pi-cog', command: () => window.location.href = '/projects' },
    { label: 'Submissions', icon: 'pi pi-cog', command: () => window.location.href = '/submissions' },
    { label: 'Contact', icon: 'pi pi-envelope', command: () => window.location.href = '/contact' }
];
</script>

<style scoped>
.navbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem 2rem;
    background-color: #333;
    color: #fff;
}

.logo {
    font-size: 1.5rem;
    font-weight: bold;
    text-decoration: none;
    color: #fff;
}
</style>
