<template>
  <div class="bid-history">
    <h3>Bid History</h3>
    <div class="bids" v-for="bid in bids" :key="bid.timestamp">
      <div :class="{ 'top-bid': bid.amount === topBid }">
        <strong>{{ bid.bidder }}</strong> bid ${{ bid.amount }}
        <span>{{ new Date(bid.timestamp).toLocaleString() }}</span>
      </div>
    </div>
  </div>
</template>

<script>
import { onMounted, ref } from 'vue';
import { useSignalR } from '../realtime/signalr'; // Adjusted import path

export default {
  props: ['auctionId'],
  setup(props) {
    const bids = ref([]);
    const topBid = ref(0);

    const { connection } = useSignalR();

    onMounted(() => {
      connection.on('ReceiveBidHistory', (data) => {
        bids.value = data;
        topBid.value = Math.max(...data.map(b => b.amount));
      });

      connection.invoke('JoinAuctionGroup', props.auctionId);
    });

    return { bids, topBid };
  }
};
</script>

<style scoped>
.top-bid {
  color: green;
  font-weight: bold;
}
.bid-history {
  margin-top: 1rem;
}
</style>
