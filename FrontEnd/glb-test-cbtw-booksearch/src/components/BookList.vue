<script setup lang="ts">
import Message from 'primevue/message'
import Skeleton from 'primevue/skeleton'
import BookCard from './BookCard.vue'
import type { Book } from '@/api/booksApi'

defineProps<{
  books: Book[]
  loading: boolean
  errorMessage: string | null
  // Shown when there are no books; null shows nothing, eg. before the first search
  emptyMessage: string | null
}>()
</script>

<template>
  <section class="book-list">
    <template v-if="loading">
      <Skeleton v-for="skeletonIndex in 4" :key="skeletonIndex" height="8.5rem" />
    </template>

    <Message v-else-if="errorMessage" severity="error">{{ errorMessage }}</Message>

    <Message v-else-if="books.length === 0 && emptyMessage" severity="secondary">
      {{ emptyMessage }}
    </Message>

    <TransitionGroup v-else name="rise" tag="div" class="book-cards" appear>
      <BookCard
        v-for="(book, bookIndex) in books"
        :key="book.id"
        :book="book"
        :style="{ '--stagger-index': bookIndex }"
      />
    </TransitionGroup>
  </section>
</template>

<style scoped>
.book-list,
.book-cards {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
</style>
