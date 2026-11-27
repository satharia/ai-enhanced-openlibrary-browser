<script setup lang="ts">
import Button from 'primevue/button'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import BookCover from './BookCover.vue'
import type { Book } from '@/api/booksApi'

defineProps<{ book: Book }>()
</script>

<template>
  <Card class="book-card lift-on-hover" :class="{ 'best-match': book.isPrimary }">
    <template #content>
      <div class="book-row">
        <BookCover :url="book.coverUrl" :title="book.title" />

        <div class="details">
          <div class="title-line">
            <a
              :href="`https://openlibrary.org/works/${book.id}`"
              target="_blank"
              rel="noopener"
              class="title"
            >
              {{ book.title }}
            </a>
            <Tag v-if="book.isPrimary" value="Best match" severity="success" />
          </div>

          <div class="meta">
            <span v-if="book.authors.length === 0">Unknown author</span>
            <template v-for="(author, authorIndex) in book.authors" :key="author.id ?? author.name">
              <span v-if="authorIndex > 0">, </span>
              <RouterLink
                v-if="author.id"
                :to="{ name: 'author', params: { authorId: author.id } }"
                class="author-link"
              >
                {{ author.name }}
              </RouterLink>
              <span v-else>{{ author.name }}</span>
            </template>
            <span v-if="book.firstPublishYear"> &middot; {{ book.firstPublishYear }}</span>
            <span v-if="book.editionCount"> &middot; {{ book.editionCount }} editions</span>
          </div>

          <p v-if="book.explanation" class="explanation">{{ book.explanation }}</p>

          <Button
            as="router-link"
            :to="{ name: 'editions', params: { workId: book.id } }"
            label="All editions"
            size="small"
            variant="text"
            class="editions-button"
          />
        </div>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.book-card.best-match {
  outline: 2px solid var(--p-green-400);
}

.book-row {
  display: flex;
  gap: 1rem;
}

.details {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.25rem;
  min-width: 0;
}

.title-line {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.title {
  font-family: var(--heading-font);
  font-size: 1.1rem;
  font-weight: 600;
  color: inherit;
  text-decoration: none;
}

.title:hover,
.author-link:hover {
  text-decoration: underline;
}

.meta {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}

.author-link {
  color: var(--p-primary-color);
  text-decoration: none;
}

.explanation {
  margin: 0.25rem 0 0;
  font-size: 0.9rem;
}

.editions-button {
  margin-left: -0.75rem;
}
</style>
