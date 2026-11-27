<script setup lang="ts">
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import ChevronLeftIcon from '@primevue/icons/chevronleft'

const router = useRouter()

// Opened from a shared link there is no previous page in the app, so it goes to the search instead
function goBack() {
  if (window.history.state?.back) {
    router.back()
  } else {
    router.push({ name: 'home' })
  }
}
</script>

<template>
  <Button label="Back" variant="text" size="small" class="back-link" @click="goBack">
    <template #icon>
      <ChevronLeftIcon />
    </template>
  </Button>
</template>

<style scoped>
.back-link {
  align-self: flex-start;
  margin-left: -0.75rem;
}
</style>
