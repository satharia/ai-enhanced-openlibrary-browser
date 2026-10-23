<script setup lang="ts">
import { ref } from 'vue'
import InputGroup from 'primevue/inputgroup'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import SearchIcon from '@primevue/icons/search'

defineProps<{ loading: boolean }>()
const emit = defineEmits<{ search: [text: string] }>()

const text = ref('')

function submit() {
  const value = text.value.trim()
  if (value) emit('search', value)
}
</script>

<template>
  <form class="search-bar" @submit.prevent="submit">
    <InputGroup>
      <InputText
        v-model="text"
        placeholder="A title, an author, or just describe the book"
        size="large"
        autofocus
      />
      <Button type="submit" label="Search" size="large" :loading="loading" :disabled="!text.trim()">
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
