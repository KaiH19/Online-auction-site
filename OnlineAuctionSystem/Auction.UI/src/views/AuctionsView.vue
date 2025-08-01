<template>
  <main class="container" style="max-width: 900px; margin: 2rem auto;">
    <h1>Auctions</h1>

    <div v-if="loading">Loading auctions…</div>
    <div v-else-if="error" style="color: #c00;">{{ error }}</div>

    <div v-else class="list" style="display: grid; gap: 1rem;">
      <article
        v-for="a in auctions"
        :key="a.id"
        class="card"
        style="border: 1px solid #ddd; border-radius: 8px; padding: 1rem;"
      >
        <header style="display:flex; justify-content: space-between; align-items: baseline;">
          <h2 style="margin:0;">{{ a.title }}</h2>
          <small>Seller: {{ a.sellerEmail }}</small>
        </header>

        <p v-if="a.description" style="margin: .5rem 0 1rem;">{{ a.description }}</p>

        <div style="display:flex; gap: 2rem; align-items: center; flex-wrap: wrap;">
          <div>
            <div><strong>Current:</strong> {{ formatCurrency(a.currentPrice) }}</div>
            <div><strong>Start:</strong> {{ formatCurrency(a.startPrice) }}</div>
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
import { onMounted, onUnmounted, ref } from 'vue';
import Countdown from '@/components/Countdown.vue';
import type { AuctionDto } from '@/types/auction';

// ✅ Real-time helpers (make sure you added src/realtime/signalr.ts)
import {
  getConnection,
  joinAuctionRoom,
  leaveAuctionRoom,
  onBidPlaced,
  onAuctionClosed,
} from '@/realtime/signalr';

// Configure your API base URL (via .env)
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

    // Ensure SignalR connection and join rooms for all currently visible auctions
    await getConnection();
    for (const a of auctions.value) {
      await joinAuctionRoom(a.id);
    }
  } catch (e: any) {
    error.value = e?.message ?? String(e);
  } finally {
    loading.value = false;
  }
}

function onExpire(id: number) {
  // When a countdown hits 0, optionally refresh this auction or the list.
  // For simplicity reload the list; the server will finalize if not already:
  load();
}

function formatCurrency(amount: number) {
  // Adjust locale/currency to your preference
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(amount);
}

// ---- Real-time handlers ----

function applyBidUpdate(p: {
  auctionId: number;
  bidId: number;
  amount: number;
  currentPrice: number;
  bidderEmail: string;
  timestamp: string;
}) {
  const a = auctions.value.find(x => x.id === p.auctionId);
  if (!a) return;

  // Update current price
  a.currentPrice = p.currentPrice;

  // Optionally update bids array if you’re displaying bids in this view
  a.bids.unshift({
    id: p.bidId,
    amount: p.amount,
    timestamp: p.timestamp,
    bidderEmail: p.bidderEmail,
  });
}

function applyClosedUpdate(p: {
  auctionId: number;
  finalPrice: number;
  winnerEmail?: string | null;
  closedAt: string;
}) {
  const a = auctions.value.find(x => x.id === p.auctionId);
  if (!a) return;

  a.isClosed = true;
  a.currentPrice = p.finalPrice;
  a.winnerEmail = p.winnerEmail ?? null;
}

onMounted(async () => {
  await load();

  // Attach SignalR event listeners once (connection is awaited in load())
  onBidPlaced(applyBidUpdate);
  onAuctionClosed(applyClosedUpdate);
});

onUnmounted(async () => {
  // Leave all joined rooms to keep the hub clean
  for (const a of auctions.value) {
    await leaveAuctionRoom(a.id);
  }
});
</script>
