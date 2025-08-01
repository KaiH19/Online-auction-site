<template>
  <main class="container" style="max-width: 900px; margin: 2rem auto;">
    <h1>Auctions</h1>

    <div v-if="loading">Loading auctions…</div>
    <div v-else-if="error" style="color: #c00;">{{ error }}</div>

    <div v-else class="list" style="display: grid; gap: 1rem;">
      <article v-for="a in auctions" :key="a.id" class="card" style="border: 1px solid #ddd; border-radius: 8px; padding: 1rem;">
        <header style="display:flex; justify-content: space-between; align-items: baseline;">
          <h2 style="margin:0;">{{ a.title }}</h2>
          <small>Seller: {{ a.sellerEmail }}</small>
        </header>

        <p v-if="a.description" style="margin: .5rem 0 1rem;">{{ a.description }}</p>

        <div style="display:flex; gap: 2rem; align-items: center; flex-wrap: wrap;">
          <div>
            <div><strong>Current:</strong> {{ a.currentPrice }}</div>
            <div><strong>Start:</strong> {{ a.startPrice }}</div>
          </div>
          <div>
            <strong>Status:</strong>
            <span v-if="a.isClosed">Closed</span>
            <span v-else>Open</span>
          </div>
          <div>
            <strong>Ends in:</strong>
            <Countdown
              :endTime="a.endTime"
              :serverTimeUtc="a.serverTimeUtc"
              :onExpire="() => onExpire(a.id)"
            />
          </div>
        </div>

        <footer style="margin-top: .75rem; font-size: .9rem; color: #555;">
          <div><strong>Ends:</strong> {{ new Date(a.endTime).toLocaleString() }}</div>
          <div v-if="a.winnerEmail"><strong>Winner:</strong> {{ a.winnerEmail }}</div>
        </footer>
      </article>
    </div>
  </main>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import Countdown from '@/components/Countdown.vue';
import type { AuctionDto } from '@/types/auction';

// Configure your API base URL. Prefer .env (see below).
const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5030';

const auctions = ref<AuctionDto[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);

async function load() {
  loading.value = true;
  error.value = null;
  try {
    const res = await fetch(`${API_BASE}/api/auctions`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = (await res.json()) as AuctionDto[];
    auctions.value = data;
  } catch (e: any) {
    error.value = e?.message ?? String(e);
  } finally {
    loading.value = false;
  }
}

function onExpire(id: number) {
  // Optionally re-fetch that auction or the list once a timer hits zero
  // For now, refresh the list:
  load();
}

onMounted(load);
</script>
