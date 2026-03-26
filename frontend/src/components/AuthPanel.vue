<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { Eye, EyeOff } from 'lucide-vue-next'
import BaseButton from './base/BaseButton.vue'

const props = defineProps<{
  mode: 'login' | 'register'
  loading: boolean
}>()

const emit = defineEmits<{
  (e: 'change-mode', value: 'login' | 'register'): void
  (e: 'submit', payload: { username: string; password: string }): void
}>()

const form = reactive({
  username: '',
  password: '',
})
const showPassword = ref(false)

watch(
  () => props.mode,
  () => {
    form.password = ''
    showPassword.value = false
  },
)

function submit() {
  emit('submit', {
    username: form.username.trim(),
    password: form.password,
  })
}
</script>

<template>
  <section class="mx-auto max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-lg">
    <div class="mb-5 flex rounded-xl bg-slate-100 p-1">
      <BaseButton
        class="w-1/2"
        :variant="mode === 'login' ? 'secondary' : 'ghost'"
        @click="emit('change-mode', 'login')"
      >
        Login
      </BaseButton>
      <BaseButton
        class="w-1/2"
        :variant="mode === 'register' ? 'secondary' : 'ghost'"
        @click="emit('change-mode', 'register')"
      >
        Register
      </BaseButton>
    </div>

    <form class="space-y-4" @submit.prevent="submit">
      <div>
        <label class="mb-1 block text-sm font-medium text-slate-700">Username</label>
        <input
          v-model="form.username"
          class="w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100"
          placeholder="your_username"
          autocomplete="username"
          maxlength="50"
          type="text"
        />
      </div>
      <div>
        <label class="mb-1 block text-sm font-medium text-slate-700">Password</label>
        <div class="relative">
          <input
            v-model="form.password"
            class="w-full rounded-xl border border-slate-300 bg-white px-3 py-2 pr-11 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100"
            placeholder="******"
            autocomplete="current-password"
            maxlength="128"
            :type="showPassword ? 'text' : 'password'"
          />
          <button
            class="absolute inset-y-0 right-0 my-1 mr-1 inline-flex items-center rounded-lg px-2 text-slate-500 transition hover:bg-slate-100 hover:text-slate-700"
            type="button"
            :aria-label="showPassword ? 'Hide password' : 'Show password'"
            @click="showPassword = !showPassword"
          >
            <Eye v-if="!showPassword" :size="16" />
            <EyeOff v-else :size="16" />
          </button>
        </div>
      </div>

      <BaseButton :loading="loading" full-width type="submit" variant="primary">
        {{ mode === 'login' ? 'Login' : 'Create account' }}
      </BaseButton>
    </form>
  </section>
</template>
