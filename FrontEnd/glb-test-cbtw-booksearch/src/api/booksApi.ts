// Client for GET /api/books/search. Only the fields the UI shows are typed; the response also
// carries criteria, tier and summary, which are debugging data and stay out of the UI.

export interface Book {
  id: string
  title: string
  authors: { id: string | null; name: string }[]
  firstPublishYear: number | null
  coverUrl: string | null
  isPrimary: boolean
  explanation: string | null
}

export interface SearchResponse {
  books: { items: Book[]; total: number }
  [debug: string]: unknown
}

// In dev the Vite proxy serves /api from the same origin; a build calls the configured URL.
const baseUrl = import.meta.env.DEV ? '' : import.meta.env.VITE_API_BASE_URL

export async function searchBooks(
  searchText: string,
  signal?: AbortSignal,
): Promise<SearchResponse> {
  const params = new URLSearchParams({ searchText })
  const response = await fetch(`${baseUrl}/api/books/search?${params}`, { signal })

  if (!response.ok) {
    throw new Error(`Search failed (${response.status})`)
  }

  return response.json()
}
