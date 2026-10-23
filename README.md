# Book Search

AI-enhanced Open Library browser: a .NET API that interprets a free-text search with Gemini, queries Open Library, ranks the results, and a Vue front end to use it.

## Design choices

- **Gemini LLM usage.** There are two places I identified as the most valuable in this test to apply the virtues of an LLM: first, to interpret the user's intent in their free-text search; second, to rank the Open Library results that did not fall into one of the deterministic ranking rules.

## Things I would do with more time

- **Cache.** I left caching for the end. My strategy was to handle it as a behaviour in the CQRS pipeline; I did not have the time to implement and test it properly, so I left it out, a critical failure in my time management.
- **Hand-crafted front end.** I wanted to build a beautifully animated front-end experience, something I would do with more time and calm.
- **Unit testing.** I wanted to write unit tests for each edge case of the deterministic ranker. My approach was to build mocks with hard-to-handle responses to ensure the ranker behaved correctly, especially when Open Library's search results list multiple authors.
- **Author works page**: I wanted to add a page with all of an Author's works by clicking their name on the results list, this would've used the Author endpoint provided by OpenLibrary to show all works by an autor ranked by the number of editions each work has (a way to infer the popularity of a book, popular books tend to have more editions and publication dates)
- **All  editions of a book**: I wanted to add another details view by clicking on a button I would've added below the book name to see all editions of a book with their covers

## Issues during the test

- I wanted to showcase my knowledge of clean architecture and the CQRS pattern, so I decided to write the solution by hand, something I ended up regretting, as the boilerplate took longer than I hoped.
- I spent a long time designing the solution and understanding the data set Open Library provides. The ranker was a little rushed and I did not implement the test suite I would have liked to.
- I did not put as much time into the front end as I wanted. It too was rushed, and I resorted to Claude Code to write that code, something I did not want to do for this test.
- Debugging took me a long time, and I resorted to Claude Code to find errors in the ranker at the end, something I did not want to do for this test.

## Setup

- **Redis (skipped).** A Docker Compose file is included to create the Redis cache container, but the cache was not integrated due to time constraints, a failure on my part.
- **Gemini API key.** The application takes the API key from the .NET DI container as a configuration property. Locally it comes from the user secrets of the API project, under the key `Gemini:ApiKey`. In cloud environments it comes from an environment variable named `Gemini__ApiKey` (in AWS ECS it is meant to be sourced from Secrets Manager through the task definition's `secrets` option).
- **Open Library identity.** Open Library rate-limits all requests, and more strictly those without identification headers. The app name and contact email are pulled from the same user secrets under `OpenLibrary:AppName` and `OpenLibrary:ContactEmail`; the environment variable equivalents are `OpenLibrary__AppName` and `OpenLibrary__ContactEmail`.

## Running the back end

Requirements: .NET 10 SDK.

1. Set the secrets once, from `BackEnd/GLB.TechnicalTest.CBTW.BookSearch`:

   ```bash
   dotnet user-secrets set "Gemini:ApiKey" "YOUR_KEY" --project GLB.TechnicalTest.CBTW.BookSearch.API
   dotnet user-secrets set "OpenLibrary:AppName" "BookSearch/1.0" --project GLB.TechnicalTest.CBTW.BookSearch.API
   dotnet user-secrets set "OpenLibrary:ContactEmail" "you@example.com" --project GLB.TechnicalTest.CBTW.BookSearch.API
   ```

2. Run the API:

   ```bash
   dotnet run --project GLB.TechnicalTest.CBTW.BookSearch.API --launch-profile http
   ```

   It listens on `http://localhost:5138`. The OpenAPI document is at `/openapi/v1.json`.

3. Try it:

   ```bash
   curl "http://localhost:5138/api/books/search?searchText=hobbit%20by%20tolkien"
   ```

Without a Gemini key the API still runs; searches then use the raw text and the deterministic ranking only.

## Running the front end

Requirements: Node 22.18+ or 24.12+.

1. From `FrontEnd/glb-test-cbtw-booksearch`:

   ```bash
   npm install
   ```

2. Copy `.env.example` to `.env.local` and set:

   | Variable | Purpose |
   |---|---|
   | `VITE_API_BASE_URL` | Back-end URL, `http://localhost:5138` locally. In `npm run dev` the Vite proxy forwards `/api` to it, so no CORS is needed. |
   | `VITE_PRIMEUI_LICENSE` | PrimeUI license key for PrimeVue 5. Without one a small "Invalid PrimeUI License" badge is shown. |

3. Start it:

   ```bash
   npm run dev
   ```

   Open `http://localhost:5173`, type a title, an author, or a description, and press Enter.
