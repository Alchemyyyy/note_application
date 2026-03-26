<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    variant?: 'primary' | 'secondary' | 'danger' | 'ghost'
    size?: 'sm' | 'md'
    type?: 'button' | 'submit' | 'reset'
    loading?: boolean
    disabled?: boolean
    fullWidth?: boolean
  }>(),
  {
    variant: 'primary',
    size: 'md',
    type: 'button',
    loading: false,
    disabled: false,
    fullWidth: false,
  },
)

const classes = computed(() => {
  const base = 'inline-flex items-center justify-center rounded-xl font-semibold transition disabled:cursor-not-allowed disabled:opacity-60'

  const variantMap: Record<string, string> = {
    primary: 'bg-emerald-600 text-white hover:bg-emerald-700',
    secondary: 'border border-slate-300 bg-white text-slate-700 hover:border-slate-400',
    danger: 'border border-rose-300 bg-rose-50 text-rose-700 hover:bg-rose-100',
    ghost: 'text-slate-600 hover:bg-slate-100 hover:text-slate-800',
  }

  const sizeMap: Record<string, string> = {
    sm: 'px-3 py-1.5 text-xs',
    md: 'px-4 py-2 text-sm',
  }

  return [
    base,
    variantMap[props.variant],
    sizeMap[props.size],
    props.fullWidth ? 'w-full' : '',
  ]
})
</script>

<template>
  <button :class="classes" :disabled="disabled || loading" :type="type">
    <span v-if="loading">Please wait...</span>
    <slot v-else />
  </button>
</template>
