# Book Search

AI-enhanced Open Library browser. A .NET API reads a free-text search with Gemini, queries [Open Library](https://openlibrary.org/developers/api), ranks the results, and a Vue front end lets you browse them, an author's works and every edition of a book.

## What it does

- **Understands free-text searches.** "the hobbit by tolkien", "roman history books from the 90s" or just "rowling" are turned by Gemini into structured search criteria (title, author, subject, publisher, language, publish year range). Anything it cannot classify stays as free text.
- **Ranks the results.** A deterministic ranker sorts Open Library's results by title similarity and by where the wanted author appears in the author list. Gemini is only asked to rank when no result is a clear match. Every result carries a one-line explanation of why it was ranked there.
- **Never fails because of the LLM.** If Gemini fails or returns invalid criteria, the search runs with the raw text; if the structured search finds nothing, it is retried as free text; if the Gemini ranking fails, the deterministic ranking is used.
- **Author page.** Clicking an author shows their bio and all their works, sorted by number of editions: popular books tend to be published again and again, so it is a good proxy for popularity.
- **All editions of a book.** Every result has an "All editions" button that lists every edition of the work with its cover, publisher, date, pages, language and ISBN.
- **Caching.** Every query is cached for a while (searches 1 hour, author and edition pages 1 day) in Redis, or in memory when Redis is not configured. Open Library rate-limits its API, so repeated searches never hit it again.

## Architecture

![Architecture](<Docs/LLM enhanced OpenLibrary search-Architecture.png>)

The back end follows Clean Architecture with a hand-written CQRS mediator:

| Project | Responsibility |
|---|---|
| `API` | Controllers. They only turn HTTP requests into queries and send them through `ISender`. |
| `Application` | Queries, handlers, the deterministic `MatchRanker` and the pipeline behaviours. Defines the interfaces it needs (`IBookCatalog`, `ISearchIntentExtractor`, `IResultRanker`). |
| `Domain` | `BookSearchCriteria` and its validation rules, shared exceptions and utilities. |
| `Infrastructure` | Open Library client (rate limited, with retries) and the Gemini implementations of the intent extractor and the ranker. |
| `Application.Tests` | xUnit tests for the ranker, the caching behaviour and the search handler. |

A request goes `Controller → ISender → CachingBehavior → Handler`. Any query that implements `ICachedQuery` (a cache key and a TTL) is cached by `CachingBehavior` without the handler knowing about it.

![Main logic flow](<Docs/LLM enhanced OpenLibrary search-Main Logic Flow.png>)

### Ranking tiers

| Tier | When | Gemini call |
|---|---|---|
| `ExactTitleAndPrimaryAuthor` | Same title, and the wanted author is the primary (first) author or no author was asked for | No |
| `ExactTitleAndContributor` | Same title, but the wanted author is only a contributor (eg. an illustrated edition) | No |
| `AuthorOnly` | Only an author was asked for: their work with the most editions comes first | No |
| `NearMatch` | A similar title (shared words, or one title inside the other) by the wanted author | Yes, to confirm or replace the closest title |
| `NoClearWinner` | Nothing above | Yes, it picks the best candidates or none |

Titles are compared ignoring case, punctuation and a leading "The", since Open Library is not consistent with them.

## Design choices

- **Gemini LLM usage.** There are two places where an LLM adds the most value here: first, to interpret the user's intent in their free-text search; second, to rank the Open Library results that did not fall into one of the deterministic ranking rules. Every other decision is deterministic, cheaper and testable.
- **Structured output.** Gemini is constrained to answer with JSON matching a schema generated from the C# response types, so the expected response shape and the code can not drift apart.
- **Open Library etiquette.** Requests are identified with an app name and contact email, limited to 3 per second, and only ask for the fields the app uses.

## API

| Endpoint | Returns |
|---|---|
| `GET /api/books/search?searchText=&page=&pageSize=` | The criteria used, whether the free-text fallback was used, the ranking tier and summary, and the ranked books |
| `GET /api/books/{workId}/editions?page=&pageSize=` | The work (title, description, subjects, cover) and a page of its editions |
| `GET /api/authors/{authorId}/works?page=&pageSize=` | The author (bio, dates, photo) and a page of their works, most editions first |

Ids are Open Library ids (`OL27482W` for a work, `OL26320A` for an author). Unknown ids return `404`. Requests for the API are in [`GLB.TechnicalTest.CBTW.BookSearch.API.http`](BackEnd/GLB.TechnicalTest.CBTW.BookSearch/GLB.TechnicalTest.CBTW.BookSearch.API/GLB.TechnicalTest.CBTW.BookSearch.API.http).

## Running it

### Configuration

- **Gemini API key (required).** Read from configuration under `Gemini:ApiKey`; the API refuses to start without it. Locally it comes from the API project's user secrets. In cloud environments it comes from an environment variable named `Gemini__ApiKey` (in AWS ECS it is meant to be sourced from Secrets Manager through the task definition's `secrets` option).
- **Open Library identity.** Open Library rate-limits all requests, and more strictly those without identification. The app name and contact email are read from `OpenLibrary:AppName` and `OpenLibrary:ContactEmail` (environment variables `OpenLibrary__AppName` and `OpenLibrary__ContactEmail`).
- **Redis (optional).** Set the connection string `ConnectionStrings:Redis` (eg. `localhost:6379`) to cache in Redis. Without it the cache lives in the API's memory.

### Back end

Requirements: .NET 10 SDK (the solution uses the `.slnx` format) and the .NET 8 runtime.

1. Set the secrets once, from `BackEnd/GLB.TechnicalTest.CBTW.BookSearch`:

   ```bash
   dotnet user-secrets set "Gemini:ApiKey" "YOUR_KEY" --project GLB.TechnicalTest.CBTW.BookSearch.API
   dotnet user-secrets set "OpenLibrary:AppName" "BookSearch/1.0" --project GLB.TechnicalTest.CBTW.BookSearch.API
   dotnet user-secrets set "OpenLibrary:ContactEmail" "you@example.com" --project GLB.TechnicalTest.CBTW.BookSearch.API
   ```

2. Optionally start Redis and point the API at it:

   ```bash
   docker compose up -d redis
   dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379" --project GLB.TechnicalTest.CBTW.BookSearch.API
   ```

3. Run the API:

   ```bash
   dotnet run --project GLB.TechnicalTest.CBTW.BookSearch.API --launch-profile http
   ```

   It listens on `http://localhost:5138`, with Swagger UI at `http://localhost:5138/swagger`.

4. Run the tests:

   ```bash
   dotnet test
   ```

### Back end with Docker

From the repository root, this builds the API and starts it with Redis on `http://localhost:5138`:

```bash
GEMINI_API_KEY=YOUR_KEY OPENLIBRARY_CONTACT_EMAIL=you@example.com docker compose up --build
```

### Front end

Requirements: Node 22.18+ or 24.12+.

1. From `FrontEnd/glb-test-cbtw-booksearch`:

   ```bash
   npm install
   ```

2. Optionally copy `.env.example` to `.env.local`:

   | Variable | Purpose |
   |---|---|
   | `VITE_API_BASE_URL` | API URL, `http://localhost:5138` by default. In `npm run dev` the Vite proxy forwards `/api` to it, so no CORS is needed. |
   | `VITE_PRIMEUI_LICENSE` | PrimeUI license key for PrimeVue 5. Without one a small "Invalid PrimeUI License" badge is shown. |

3. Start it:

   ```bash
   npm run dev
   ```

   Open `http://localhost:5173`, type a title, an author, or a description, and press Enter.

## Background

This started as a time-boxed technical test. Caching, the unit tests, the author and editions pages and the front-end polish were finished afterwards.
