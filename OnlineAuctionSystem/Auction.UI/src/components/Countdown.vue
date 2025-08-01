<template>
  <span :title="new Date(endMs).toLocaleString()">{{ formatted }}</span>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watchEffect } from 'vue';

const props = defineProps<{
  endTime: string;        // ISO from API
  serverTimeUtc: string;  // ISO from API
  onExpire?: () => void;  // optional callback
}>();

const endMs = ref(0);
const skewMs = ref(0);
const remaining = ref(0);
let intervalId: number | null = null;

function tick() {
  const secs = Math.max(
    0,
    Math.floor((endMs.value - (Date.now() + skewMs.value)) / 1000)
  );
  remaining.value = secs;
  if (secs === 0 && intervalId) {
    clearInterval(intervalId);
    intervalId = null;
    if (props.onExpire) props.onExpire();
  }
}

watchEffect(() => {
  endMs.value = new Date(props.endTime).getTime();
  const serverMs = new Date(props.serverTimeUtc).getTime();
  skewMs.value = serverMs - Date.now(); // sync to server clock
  tick();
});

onMounted(() => {
  intervalId = window.setInterval(tick, 1000);
});

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId);
});

const formatted = computed(() => {
  const s = remaining.value;
  const h = Math.floor(s / 3600);
  const m = Math.floor((s % 3600) / 60);
  const sec = s % 60;
  return `${String(h).padStart(2,'0')}:${String(m).padStart(2,'0')}:${String(sec).padStart(2,'0')}`;
});
</script>
