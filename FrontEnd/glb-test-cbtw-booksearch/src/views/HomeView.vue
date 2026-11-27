<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import BookList from '@/components/BookList.vue'
import SearchBar from '@/components/SearchBar.vue'
import { searchBooks } from '@/api/booksApi'
import { useRequest } from '@/composables/useRequest'

const route = useRoute()
const router = useRouter()

// The search text lives in the URL (?q=...) so going back from an author or editions page shows the same results
const searchText = computed(() => (typeof route.query.q === 'string' ? route.query.q : ''))

const {
  data: searchResult,
  isLoading,
  errorMessage,
  run: runSearch,
} = useRequest((signal) => searchBooks(searchText.value, signal))

watch(
  searchText,
  (newSearchText) => {
    if (newSearchText) runSearch()
  },
  { immediate: true },
)

function search(newSearchText: string) {
  // Searching the same text again does not change the URL, so it has to run explicitly
  if (newSearchText === searchText.value) {
    runSearch()
  } else {
    router.push({ name: 'home', query: { q: newSearchText } })
  }
}
</script>

<template>
  <main class="home">
    <header class="hero">
      <h1>Find a book</h1>
      <p class="subtitle">
        Type a title, an author, or describe the book you have in mind. Gemini reads what you meant,
        Open Library finds the books.
      </p>
    </header>

    <SearchBar :initial-text="searchText" :loading="isLoading" @search="search" />

    <Transition name="fade">
      <p v-if="searchResult && !isLoading && !errorMessage" class="summary">
        {{ searchResult.summary }}
      </p>
    </Transition>

    <BookList
      :books="searchResult?.books.items ?? []"
      :loading="isLoading"
      :error-message="errorMessage"
      :empty-message="searchResult ? 'No books found. Try fewer words, or just the title.' : null"
    />
  </main>
</template>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.hero h1 {
  margin: 0;
  font-size: 2.25rem;
}

.subtitle {
  margin: 0.5rem 0 0;
  color: var(--p-text-muted-color);
}

.summary {
  margin: 0;
  font-style: italic;
  color: var(--p-text-muted-color);
}
</style>
