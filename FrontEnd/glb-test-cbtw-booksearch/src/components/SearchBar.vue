<script setup lang="ts">
import { ref, watch } from 'vue'
import Button from 'primevue/button'
import InputGroup from 'primevue/inputgroup'
import InputText from 'primevue/inputtext'
import SearchIcon from '@primevue/icons/search'

const props = defineProps<{ initialText: string; loading: boolean }>()
const emit = defineEmits<{ search: [searchText: string] }>()

const searchText = ref(props.initialText)

// Keeps the input in sync when the search comes from the URL, eg. going back in the browser history
watch(
  () => props.initialText,
  (initialText) => (searchText.value = initialText),
)

function submitSearch() {
  const trimmedText = searchText.value.trim()
  if (trimmedText) emit('search', trimmedText)
}
</script>

<template>
  <form class="search-bar" @submit.prevent="submitSearch">
    <InputGroup>
      <InputText
        v-model="searchText"
        placeholder="A title, an author, or just describe the book"
        size="large"
        autofocus
      />
      <Button
        type="submit"
        label="Search"
        size="large"
        :loading="loading"
        :disabled="!searchText.trim()"
      >
        <template #icon>
          <SearchIcon />
        </template>
      </Button>
    </InputGroup>
  </form>
</template>

<style scoped>
.search-bar {
  width: 100%;
}
</style>
