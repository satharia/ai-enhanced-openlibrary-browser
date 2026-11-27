// In development the Vite proxy serves /api from the same origin, a production build calls the configured API URL
const apiBaseUrl = import.meta.env.DEV ? '' : import.meta.env.VITE_API_BASE_URL

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, { signal })

  if (response.status === 404) {
    throw new Error('Open Library has nothing with that id')
  }

  if (!response.ok) {
    throw new Error(`Request failed (${response.status})`)
  }

  return response.json()
}
