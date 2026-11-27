<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Message from 'primevue/message'
import Paginator, { type PageState } from 'primevue/paginator'
import Skeleton from 'primevue/skeleton'
import Tag from 'primevue/tag'
import BackLink from '@/components/BackLink.vue'
import BookCover from '@/components/BookCover.vue'
import EditionCard from '@/components/EditionCard.vue'
import { getBookEditions } from '@/api/booksApi'
import { useRequest } from '@/composables/useRequest'

const props = defineProps<{ workId: string }>()

const pageSize = 24
const maxSubjects = 8
const page = ref(1)

const {
  data: bookEditions,
  isLoading,
  errorMessage,
  run: loadBookEditions,
} = useRequest((signal) => getBookEditions(props.workId, page.value, pageSize, signal))

watch([() => props.workId, page], loadBookEditions, { immediate: true })

const work = computed(() => bookEditions.value?.work ?? null)

function changePage(pageState: PageState) {
  page.value = pageState.page + 1
  window.scrollTo({ top: 0, behavior: 'smooth' })
}
</script>

<template>
  <main class="editions-page">
    <BackLink />

    <header v-if="work" class="work-header">
      <BookCover :url="work.coverUrl" :title="work.title" size="large" />
      <div class="work-details">
        <h1>{{ work.title }}</h1>
        <p v-if="work.description" class="description">{{ work.description }}</p>
        <div v-if="work.subjects.length > 0" class="subjects">
          <Tag
            v-for="subject in work.subjects.slice(0, maxSubjects)"
            :key="subject"
            :value="subject"
            severity="secondary"
          />
        </div>
      </div>
    </header>
    <Skeleton v-else-if="isLoading" height="210px" />

    <Message v-if="errorMessage" severity="error">{{ errorMessage }}</Message>

    <template v-else>
      <h2 v-if="bookEditions">{{ bookEditions.editions.total }} editions</h2>

      <div v-if="isLoading" class="edition-grid">
        <Skeleton v-for="skeletonIndex in 8" :key="skeletonIndex" height="18rem" />
      </div>

      <TransitionGroup v-else-if="bookEditions" name="rise" tag="div" class="edition-grid" appear>
        <EditionCard
          v-for="(edition, editionIndex) in bookEditions.editions.items"
          :key="edition.id"
          :edition="edition"
          :work-title="bookEditions.work.title"
          :style="{ '--stagger-index': editionIndex }"
        />
      </TransitionGroup>

      <Paginator
        v-if="bookEditions && bookEditions.editions.total > pageSize"
        :rows="pageSize"
        :total-records="bookEditions.editions.total"
        :first="(page - 1) * pageSize"
        @page="changePage"
      />
    </template>
  </main>
</template>

<style scoped>
.editions-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.work-header {
  display: flex;
  gap: 1.5rem;
  animation: rise 0.5s ease both;
}

.work-details {
  min-width: 0;
}

h1 {
  margin: 0;
  font-size: 2rem;
}

h2 {
  margin: 1rem 0 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.description {
  margin: 0.75rem 0 0;
  white-space: pre-line;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 6;
  line-clamp: 6;
  overflow: hidden;
}

.subjects {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  margin-top: 0.75rem;
}

.edition-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(170px, 1fr));
  gap: 0.75rem;
}

@media (max-width: 560px) {
  .work-header {
    flex-direction: column;
  }
}
</style>
