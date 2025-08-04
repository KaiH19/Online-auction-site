<template>
  <div class="admin-dashboard">
    <h2 class="text-2xl font-bold mb-4">Admin Dashboard</h2>

    <div v-if="summary">
      <p>Total Auctions: {{ summary.totalAuctions }}</p>
      <p>Completed: {{ summary.completedAuctions }}</p>
      <p>Paid: {{ summary.paidAuctions }}</p>
      <p>Awaiting Payment: {{ summary.awaitingPayment }}</p>
      <p>Total Bids: {{ summary.totalBids }}</p>
      <p>Total Volume: ${{ summary.totalBidVolume.toFixed(2) }}</p>

      <h3 class="text-lg mt-4 font-semibold">Winners</h3>
      <ul>
        <li v-for="w in summary.winners" :key="w.id">
          Auction #{{ w.id }} - {{ w.title }} — {{ w.winnerEmail }} (${{ w.finalPrice }})
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const summary = ref(null);

onMounted(async () => {
  try {
    const res = await axios.get('/api/auctions/admin-summary');
    summary.value = res.data;
  } catch (err) {
    console.error('Failed to load admin summary:', err);
  }
});
</script>

<style scoped>
.admin-dashboard {
  padding: 2rem;
  background-color: #f8fafc;
  border-radius: 0.5rem;
}
</style>
