const BACKEND_URL = import.meta.env.VITE_BACKEND_URL;

// TODO: replace all direct uses of ApiService with a specialised service to centralise endpoint paths - maybe also look at generating the services (open api or something)
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

    static put<T>(path: string, data: unknown): Promise<T> {
        return this.#makeRequest(path, 'PUT', JSON.stringify(data), {
            "Content-Type": "application/json",
        })
    }

    static makeUrl(path: string): URL {
        return new URL(path, BACKEND_URL)
    }

    static async #makeRequest(path: string, method: string, body: BodyInit | undefined = undefined, headers: HeadersInit = []){
        const options: RequestInit = {
            method: method,
            credentials: "include",
            headers: headers
        };
        if (body != undefined) {
            options.body = body;
        }
        const response = await fetch(this.makeUrl(path), options)
        if (!response.ok){
            if (response.status === 401) {
                // TODO: handle this better way
                alert("You are unauthorized to perform that action. Your session may have expired and need to login again, or have not been granted access to this resource.")
                window.location.href = "/"
            }
            return null;
        }
        if (response.headers.get("Content-Type")?.includes("json")) {
            return response.json()
        } else {
            return response.text()
        }
    }
}
