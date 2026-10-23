<script setup lang="ts">
import { ref } from 'vue'
import SearchBar from '@/components/SearchBar.vue'
import BookResults from '@/components/BookResults.vue'
import { searchBooks, type Book } from '@/api/booksApi'

const books = ref<Book[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const searched = ref(false)

let inFlight: AbortController | null = null

async function search(text: string) {
  inFlight?.abort()
  inFlight = new AbortController()

  loading.value = true
  error.value = null

  try {
    const { books: page, ...debug } = await searchBooks(text, inFlight.signal)
    books.value = page.items
    searched.value = true
    console.debug('search', text, debug)
  } catch (e) {
    if (e instanceof DOMException && e.name === 'AbortError') return
    error.value = e instanceof Error ? e.message : 'Search failed'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="home">
    <h1>Find a book</h1>
    <SearchBar :loading="loading" @search="search" />
    <BookResults :books="books" :loading="loading" :error="error" :searched="searched" />
  </main>
</template>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

h1 {
  margin: 0;
  font-size: 1.75rem;
  font-weight: 600;
}
</style>
