<template>
  <div class="auction-detail">
    <h2 class="text-2xl font-bold mb-2">{{ auction.title }}</h2>
    <p class="mb-1">Description: {{ auction.description }}</p>
    <p class="mb-1">Start Price: ${{ auction.startPrice.toFixed(2) }}</p>
    <p class="mb-1">Current Price: ${{ auction.currentPrice.toFixed(2) }}</p>
    <p class="mb-1">Start Time: {{ formatDate(auction.startTime) }}</p>
    <p class="mb-1">End Time: {{ formatDate(auction.endTime) }}</p>
    <p class="mb-1" v-if="auction.isClosed">Status: 
      <span class="font-semibold" :class="{'text-green-600': auction.status === 'Paid', 'text-yellow-500': auction.status !== 'Paid'}">
        {{ auction.status || 'Awaiting Payment' }}
      </span>
    </p>

    <!-- "Pay Now" button shown only to winning user -->
    <div v-if="canPayNow" class="mt-4">
      <button @click="payNow" class="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700">
        Pay Now
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import axios from 'axios';

// Accept auction and currentUser as props
const props = defineProps({
  auction: Object,
  currentUser: Object
});

// Show "Pay Now" only to winner, after auction is closed, and not yet paid
const canPayNow = computed(() => {
  const isWinner = props.auction.winnerEmail === props.currentUser.email;
  const isClosed = props.auction.isClosed === true;
  const isPaid = props.auction.status === 'Paid';
  return isWinner && isClosed && !isPaid;
});

async function payNow() {
  try {
    const response = await axios.post('/api/payments/create-checkout-session', {
      auctionId: props.auction.id,
      userId: props.currentUser.id, // make sure this is available
      amount: props.auction.currentPrice
    });

    // Redirect user to Stripe Checkout
    window.location.href = response.data.sessionUrl;
  } catch (error) {
    console.error('Stripe payment error:', error);
    alert('Payment failed. Please try again later.');
  }
}

// Utility to format timestamps
function formatDate(dateString) {
  const date = new Date(dateString);
  return date.toLocaleString();
}
</script>

<style scoped>
.auction-detail {
  padding: 1.5rem;
  background-color: #f9fafb;
  border-radius: 0.5rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}
</style>
