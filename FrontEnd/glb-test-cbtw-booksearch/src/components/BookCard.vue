<script setup lang="ts">
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import type { Book } from '@/api/booksApi'

defineProps<{ book: Book }>()
</script>

<template>
  <Card class="book">
    <template #content>
      <div class="book-row">
        <img
          v-if="book.coverUrl"
          :src="book.coverUrl"
          :alt="book.title"
          class="cover"
          loading="lazy"
        />
        <div v-else class="cover cover-empty" />

        <div class="details">
          <div class="title-line">
            <a
              :href="`https://openlibrary.org${book.id}`"
              target="_blank"
              rel="noopener"
              class="title"
            >
              {{ book.title }}
            </a>
            <Tag v-if="book.isPrimary" value="Best match" severity="success" />
          </div>
          <div class="meta">
            {{ book.authors.map((a) => a.name).join(', ') || 'Unknown author' }}
            <span v-if="book.firstPublishYear"> &middot; {{ book.firstPublishYear }}</span>
          </div>
          <p v-if="book.explanation" class="explanation">{{ book.explanation }}</p>
        </div>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.book-row {
  display: flex;
  gap: 1rem;
}

.cover {
  width: 56px;
  height: 84px;
  flex: none;
  object-fit: cover;
  border-radius: 4px;
}

.cover-empty {
  background: var(--p-surface-200);
}

.details {
  min-width: 0;
}

.title-line {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.title {
  font-weight: 600;
  color: inherit;
  text-decoration: none;
}

.title:hover {
  text-decoration: underline;
}

.meta {
  color: var(--p-text-muted-color);
  font-size: 0.9rem;
}

.explanation {
  margin: 0.5rem 0 0;
  font-size: 0.9rem;
}
</style>
