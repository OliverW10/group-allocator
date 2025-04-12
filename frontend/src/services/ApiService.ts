
const BACKEND_URL = import.meta.env.VITE_BACKEND_URL;

export default class ApiService {
    static async get<T>(path: string): Promise<T> {
        return this.#makeRequest(path, 'GET')
    }

    static delete<T>(path: string): Promise<T> {
        return this.#makeRequest(path, 'DELETE')
    }

    static post<T>(path: string, data: unknown): Promise<T> {
        return this.#makeRequest(path, 'POST', JSON.stringify(data), {
            "Content-Type": "application/json",
        })
    }

    static async postRaw<T>(path: string, body: BodyInit | undefined): Promise<T> {
        return this.#makeRequest(path, 'POST', body)
    }

    static async #makeRequest(path: string, method: string, body: BodyInit | undefined = undefined, headers: HeadersInit = []){
        const url = new URL(path, BACKEND_URL);
        const options: RequestInit = {
            method: method,
            credentials: "include",
            headers: headers
        };
        if (body != undefined) {
            options.body = body;
        }
        const response = await fetch(url, options)
        if (!response.ok){
            return undefined;
        }
        return response.json()
    }
}