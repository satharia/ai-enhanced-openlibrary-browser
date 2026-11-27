import { getJson, type PagedResult } from './httpClient'

export interface AuthorRef {
  id: string | null
  name: string
}

export interface Book {
  id: string
  title: string
  authors: AuthorRef[]
  firstPublishYear: number | null
  editionCount: number | null
  coverUrl: string | null
  isPrimary: boolean
  explanation: string | null
}

export interface SearchBooksResult {
  usedFallback: boolean
  summary: string
  books: PagedResult<Book>
}

export interface WorkDetails {
  id: string
  title: string
  description: string | null
  subjects: string[]
  coverUrl: string | null
}

export interface Edition {
  id: string
  title: string | null
  publishers: string[]
  publishDate: string | null
  numberOfPages: number | null
  isbn13: string | null
  languages: string[]
  coverUrl: string | null
}

export interface BookEditionsResult {
  work: WorkDetails
  editions: PagedResult<Edition>
}

export function searchBooks(searchText: string, signal?: AbortSignal) {
  const parameters = new URLSearchParams({ searchText })

  return getJson<SearchBooksResult>(`/api/books/search?${parameters}`, signal)
}

export function getBookEditions(
  workId: string,
  page: number,
  pageSize: number,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })

  return getJson<BookEditionsResult>(
    `/api/books/${encodeURIComponent(workId)}/editions?${parameters}`,
    signal,
  )
}
