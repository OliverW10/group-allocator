
const BACKEND_URL = import.meta.env.VITE_BACKEND_URL;

export default class ApiService {
    static async get<T>(url: string): Promise<T> {
        const response = await fetch(url, {
            credentials: "include"
        })
        return response.json()
    }

    static async post<T>(url: string, data: unknown): Promise<T> {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data),
            credentials: "include"
        })
        return response.json()
    }
}