<script setup lang="ts">
import Card from 'primevue/card'
import BookCover from './BookCover.vue'
import type { Edition } from '@/api/booksApi'

defineProps<{ edition: Edition; workTitle: string }>()
</script>

<template>
  <Card class="edition-card lift-on-hover">
    <template #content>
      <BookCover :url="edition.coverUrl" :title="edition.title ?? workTitle" size="medium" />

      <a
        :href="`https://openlibrary.org/books/${edition.id}`"
        target="_blank"
        rel="noopener"
        class="title"
      >
        {{ edition.title ?? workTitle }}
      </a>

      <dl class="facts">
        <template v-if="edition.publishDate">
          <dt>Published</dt>
          <dd>{{ edition.publishDate }}</dd>
        </template>
        <template v-if="edition.publishers.length > 0">
          <dt>Publisher</dt>
          <dd>{{ edition.publishers.join(', ') }}</dd>
        </template>
        <template v-if="edition.numberOfPages">
          <dt>Pages</dt>
          <dd>{{ edition.numberOfPages }}</dd>
        </template>
        <template v-if="edition.languages.length > 0">
          <dt>Language</dt>
          <dd>{{ edition.languages.join(', ').toUpperCase() }}</dd>
        </template>
        <template v-if="edition.isbn13">
          <dt>ISBN</dt>
          <dd>{{ edition.isbn13 }}</dd>
        </template>
      </dl>
    </template>
  </Card>
</template>

<style scoped>
.edition-card {
  height: 100%;
}

.title {
  display: block;
  margin: 0.75rem 0 0.5rem;
  font-family: var(--heading-font);
  font-weight: 600;
  color: inherit;
  text-decoration: none;
}

.title:hover {
  text-decoration: underline;
}

.facts {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 0.15rem 0.5rem;
  margin: 0;
  font-size: 0.8rem;
}

dt {
  color: var(--p-text-muted-color);
}

dd {
  margin: 0;
  overflow-wrap: anywhere;
}
</style>
