<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import Message from 'primevue/message'
import Paginator, { type PageState } from 'primevue/paginator'
import Skeleton from 'primevue/skeleton'
import BackLink from '@/components/BackLink.vue'
import BookCover from '@/components/BookCover.vue'
import BookList from '@/components/BookList.vue'
import { getAuthorWorks } from '@/api/authorsApi'
import { useRequest } from '@/composables/useRequest'

const props = defineProps<{ authorId: string }>()

const pageSize = 20
const page = ref(1)
const isBioExpanded = ref(false)

const {
  data: authorWorks,
  isLoading,
  errorMessage,
  run: loadAuthorWorks,
} = useRequest((signal) => getAuthorWorks(props.authorId, page.value, pageSize, signal))

watch([() => props.authorId, page], loadAuthorWorks, { immediate: true })

const author = computed(() => authorWorks.value?.author ?? null)

const lifeDates = computed(() => {
  if (!author.value?.birthDate) return null
  return `${author.value.birthDate} – ${author.value.deathDate ?? 'present'}`
})

function changePage(pageState: PageState) {
  page.value = pageState.page + 1
  window.scrollTo({ top: 0, behavior: 'smooth' })
}
</script>

<template>
  <main class="author-page">
    <BackLink />

    <header v-if="author" class="author-header">
      <BookCover :url="author.photoUrl" :title="author.name" size="large" class="author-photo" />
      <div class="author-details">
        <h1>{{ author.name }}</h1>
        <p v-if="lifeDates" class="life-dates">{{ lifeDates }}</p>
        <template v-if="author.bio">
          <p class="bio" :class="{ expanded: isBioExpanded }">{{ author.bio }}</p>
          <Button
            :label="isBioExpanded ? 'Show less' : 'Read more'"
            variant="text"
            size="small"
            class="bio-toggle"
            @click="isBioExpanded = !isBioExpanded"
          />
        </template>
      </div>
    </header>
    <Skeleton v-else-if="isLoading" height="210px" />

    <Message v-if="errorMessage" severity="error">{{ errorMessage }}</Message>

    <section v-else class="works">
      <h2 v-if="authorWorks">{{ authorWorks.works.total }} works, most published first</h2>
      <BookList
        :books="authorWorks?.works.items ?? []"
        :loading="isLoading"
        :error-message="null"
        empty-message="Open Library has no works for this author."
      />
      <Paginator
        v-if="authorWorks && authorWorks.works.total > pageSize"
        :rows="pageSize"
        :total-records="authorWorks.works.total"
        :first="(page - 1) * pageSize"
        @page="changePage"
      />
    </section>
  </main>
</template>

<style scoped>
.author-page,
.works {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.author-header {
  display: flex;
  gap: 1.5rem;
  animation: rise 0.5s ease both;
}

.author-photo {
  border-radius: 8px;
}

.author-details {
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

.life-dates {
  margin: 0.25rem 0 0;
  color: var(--p-text-muted-color);
}

.bio {
  margin: 0.75rem 0 0;
  white-space: pre-line;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 5;
  line-clamp: 5;
  overflow: hidden;
}

.bio.expanded {
  display: block;
}

.bio-toggle {
  margin-left: -0.75rem;
}

@media (max-width: 560px) {
  .author-header {
    flex-direction: column;
  }
}
</style>
