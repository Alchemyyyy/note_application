import { createRouter, createWebHashHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import NotesView from '../views/NotesView.vue'
import RegisterView from '../views/RegisterView.vue'

const AUTH_TOKEN_KEY = 'notes_auth_token'

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: () => (localStorage.getItem(AUTH_TOKEN_KEY) ? '/notes' : '/login'),
    },
    {
      path: '/login',
      component: LoginView,
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      component: RegisterView,
      meta: { guestOnly: true },
    },
    {
      path: '/notes',
      component: NotesView,
      meta: { requiresAuth: true },
    },
  ],
})

router.beforeEach((to) => {
  const hasToken = Boolean(localStorage.getItem(AUTH_TOKEN_KEY))

  if (to.meta.requiresAuth && !hasToken) {
    return '/login'
  }

  if (to.meta.guestOnly && hasToken) {
    return '/notes'
  }

  return true
})

export default router
