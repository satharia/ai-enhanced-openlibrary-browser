import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/authors/:authorId',
      name: 'author',
      component: () => import('../views/AuthorView.vue'),
      props: true,
    },
    {
      path: '/books/:workId/editions',
      name: 'editions',
      component: () => import('../views/BookEditionsView.vue'),
      props: true,
    },
  ],
  // Back and forward return to where the page was scrolled, any other navigation starts at the top
  scrollBehavior(to, from, savedPosition) {
    return savedPosition ?? { top: 0 }
  },
})

export default router
