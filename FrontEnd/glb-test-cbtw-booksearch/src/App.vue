<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <nav class="top-bar">
    <RouterLink :to="{ name: 'home' }" class="brand">Book Search</RouterLink>
    <span class="powered-by">Open Library &times; Gemini</span>
  </nav>

  <!-- Keyed by path, not full path, so a new search (?q=) updates the page instead of replacing it -->
  <RouterView v-slot="{ Component, route }">
    <Transition name="page" mode="out-in">
      <component :is="Component" :key="route.path" />
    </Transition>
  </RouterView>
</template>

<style scoped>
.top-bar {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-bottom: 2.5rem;
}

.brand {
  font-family: var(--heading-font);
  font-size: 1.25rem;
  font-weight: 700;
  color: inherit;
  text-decoration: none;
}

.powered-by {
  font-size: 0.8rem;
  color: var(--p-text-muted-color);
}
</style>
