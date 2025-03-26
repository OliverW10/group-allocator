import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { UserInfoDto } from '../dtos/user-info-dto'

export const useAuthStore = defineStore(
    'auth',
    () => {
        return {
            userInfo: ref<UserInfoDto | null>(null),
        }
    },
    {
        persist: true,
    }
)
