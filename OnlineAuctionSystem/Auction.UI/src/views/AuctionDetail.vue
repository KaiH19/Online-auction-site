<template>
  <div class="container">
    <h1>{{ auction?.title }}</h1>
    <p>{{ auction?.description }}</p>

    <div>
      <strong>Current Price:</strong> {{ auction?.currentPrice }}
      <br>
      <strong>Ends in:</strong>
      <Countdown :endTime="auction?.endTime" :serverTimeUtc="auction?.serverTimeUtc" />
    </div>

    <div v-if="!auction?.isClosed">
      <input type="number" v-model.number="bidAmount" />
      <button @click="placeBid">Place Bid</button>
    </div>

    <ul>
      <li v-for="b in bidHistory" :key="b.timestamp">
        💰 {{ b.amount }} by {{ b.bidderEmail }} at {{ b.timestamp }}
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getConnection, joinAuctionRoom, onBidPlaced } from '@/realtime/signalr'
import axios from 'axios'

const route = useRoute()
const id = route.params.id
const auction = ref(null)
const bidAmount = ref(0)
const bidHistory = ref([])

onMounted(async () => {
  const res = await axios.get(`/api/auctions/${id}`)
  auction.value = res.data
  bidAmount.value = auction.value.currentPrice + 1

  joinAuctionRoom(id)
  onBidPlaced((payload) => {
    if (payload.auctionId === id) {
      auction.value.currentPrice = payload.amount
      bidHistory.value.push(payload)
    }
  })
})

const placeBid = async () => {
  await axios.post(`/api/auctions/${id}/bid`, bidAmount.value)
}
</script>
