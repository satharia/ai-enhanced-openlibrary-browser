<script setup lang="ts">
import Message from 'primevue/message'
import Skeleton from 'primevue/skeleton'
import BookCard from './BookCard.vue'
import type { Book } from '@/api/booksApi'

defineProps<{
  books: Book[]
  loading: boolean
  error: string | null
  searched: boolean
}>()
</script>

<template>
  <section class="results">
    <template v-if="loading">
      <Skeleton v-for="n in 3" :key="n" height="6.5rem" />
    </template>

    <Message v-else-if="error" severity="error">{{ error }}</Message>

    <Message v-else-if="searched && books.length === 0" severity="secondary">
      No books found. Try fewer words, or just the title.
    </Message>

    <BookCard v-for="book in books" v-else :key="book.id" :book="book" />
  </section>
</template>

<style scoped>
.results {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
</style>
