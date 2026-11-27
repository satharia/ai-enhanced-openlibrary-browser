# Book Search front end

Vue 3 + PrimeVue single page app for the Book Search API. Setup and configuration are described in the [root README](../../README.md).

| Route | View |
|---|---|
| `/?q=` | Search, the search text is kept in the URL so going back restores the results |
| `/authors/:authorId` | An author's bio and works, most editions first |
| `/books/:workId/editions` | Every edition of a work |

## Scripts

```sh
npm run dev         # development server with the /api proxy
npm run build       # type-check and build for production
npm run lint        # oxlint + eslint
npm run format      # prettier
```
