<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import AuthPanel from '../components/AuthPanel.vue'
import MainLayout from '../layouts/MainLayout.vue'
import { validateAuthPayload } from '../composables/useAuth'
import { useAuthStore } from '../stores/authStore'

const router = useRouter()
const authStore = useAuthStore()
const errorMessage = ref('')
const statusMessage = ref('')

async function submit(payload: { username: string; password: string }) {
  errorMessage.value = ''
  statusMessage.value = ''

  const validationMessage = validateAuthPayload(payload)
  if (validationMessage) {
    errorMessage.value = validationMessage
    return
  }

  try {
    const message = await authStore.register(payload)
    statusMessage.value = message
    await router.push('/notes')
  } catch (error: unknown) {
    errorMessage.value = error instanceof Error ? error.message : 'Registration failed.'
  }
}
</script>

<template>
  <MainLayout subtitle="Create your account" title="Join Notes App">
    <transition name="slide-fade">
      <div
        v-if="statusMessage"
        class="mb-4 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
      >
        {{ statusMessage }}
      </div>
    </transition>
    <transition name="slide-fade">
      <div
        v-if="errorMessage"
        class="mb-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"
      >
        {{ errorMessage }}
      </div>
    </transition>

    <AuthPanel
      :loading="authStore.loading"
      mode="register"
      @change-mode="router.push('/login')"
      @submit="submit"
    />
  </MainLayout>
</template>
