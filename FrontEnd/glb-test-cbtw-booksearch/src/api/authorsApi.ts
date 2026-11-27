import type { Book } from './booksApi'
import { getJson, type PagedResult } from './httpClient'

export interface AuthorDetails {
  id: string
  name: string
  bio: string | null
  birthDate: string | null
  deathDate: string | null
  photoUrl: string | null
}

export interface AuthorWorksResult {
  author: AuthorDetails
  works: PagedResult<Book>
}

export function getAuthorWorks(
  authorId: string,
  page: number,
  pageSize: number,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })

  return getJson<AuthorWorksResult>(
    `/api/authors/${encodeURIComponent(authorId)}/works?${parameters}`,
    signal,
  )
}
