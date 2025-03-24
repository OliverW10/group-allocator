export default class ApiService {
    static async get<T>(url: string): Promise<T> {
        const response = await fetch(url)
        return response.json()
    }

    static async post<T>(url: string, data: any): Promise<T> {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        })
        return response.json()
    }
}