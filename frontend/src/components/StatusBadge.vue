<template>
  <span :class="badgeClass">
    {{ displayText }}
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  status: string;
  type?: 'invoice' | 'user' | 'account';
}

const props = withDefaults(defineProps<Props>(), {
  type: 'invoice'
});

const badgeClass = computed(() => {
  const baseClass = 'px-3 py-1 rounded-full text-sm font-semibold';
  
  if (props.type === 'invoice') {
    switch (props.status?.toLowerCase()) {
      case 'paid':
        return `${baseClass} bg-green-100 text-green-800`;
      case 'partial':
        return `${baseClass} bg-blue-100 text-blue-800`;
      case 'unpaid':
        return `${baseClass} bg-yellow-100 text-yellow-800`;
      case 'overdue':
        return `${baseClass} bg-red-100 text-red-800`;
      case 'cancelled':
        return `${baseClass} bg-gray-100 text-gray-800`;
      default:
        return `${baseClass} bg-gray-100 text-gray-800`;
    }
  }
  
  if (props.type === 'user') {
    switch (props.status?.toLowerCase()) {
      case 'active':
        return `${baseClass} bg-green-100 text-green-800`;
      case 'locked':
        return `${baseClass} bg-red-100 text-red-800`;
      default:
        return `${baseClass} bg-gray-100 text-gray-800`;
    }
  }

  if (props.type === 'account') {
    switch (props.status?.toLowerCase()) {
      case 'mustchangepassword':
        return `${baseClass} bg-orange-100 text-orange-800`;
      case 'active':
        return `${baseClass} bg-green-100 text-green-800`;
      case 'locked':
        return `${baseClass} bg-red-100 text-red-800`;
      default:
        return `${baseClass} bg-gray-100 text-gray-800`;
    }
  }

  return `${baseClass} bg-gray-100 text-gray-800`;
});

const displayText = computed(() => {
  const text = props.status || '';
  return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase();
});
</script>
