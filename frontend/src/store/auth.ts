import { defineStore } from 'pinia'
import { ref } from 'vue'
import { type UserInfo } from '../types'

export const useAuthStore = defineStore('auth', () => {
    return {
        token: ref<string | null>(null),
        userInfo: ref<UserInfo | null>(null),
    }
})
