<script setup lang="ts">
import { ref, watch } from 'vue'

const props = withDefaults(
  defineProps<{ url: string | null; title: string; size?: 'small' | 'medium' | 'large' }>(),
  { size: 'small' },
)

// Covers fade in once downloaded; a missing cover (404) falls back to the placeholder
const hasLoaded = ref(false)
const hasFailed = ref(false)

watch(
  () => props.url,
  () => {
    hasLoaded.value = false
    hasFailed.value = false
  },
)
</script>

<template>
  <div class="cover" :class="size">
    <img
      v-if="url && !hasFailed"
      :src="url"
      :alt="`Cover of ${title}`"
      :class="{ loaded: hasLoaded }"
      loading="lazy"
      @load="hasLoaded = true"
      @error="hasFailed = true"
    />
  </div>
</template>

<style scoped>
.cover {
  flex: none;
  overflow: hidden;
  border-radius: 4px;
  background: linear-gradient(135deg, var(--p-surface-200), var(--p-surface-300));
  box-shadow: 0 1px 3px rgb(0 0 0 / 0.15);
}

@media (prefers-color-scheme: dark) {
  .cover {
    background: linear-gradient(135deg, var(--p-surface-700), var(--p-surface-800));
  }
}

.small {
  width: 64px;
  height: 96px;
}

.medium {
  width: 100%;
  aspect-ratio: 2 / 3;
}

.large {
  width: 140px;
  height: 210px;
}

img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0;
  transition: opacity 0.4s ease;
}

img.loaded {
  opacity: 1;
}
</style>
