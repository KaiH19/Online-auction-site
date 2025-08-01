export interface BidDto {
  id: number;
  amount: number;
  timestamp: string; // ISO
  bidderEmail: string;
}

export interface AuctionDto {
  id: number;
  title: string;
  description?: string | null;
  startTime: string;       // ISO
  endTime: string;         // ISO
  startPrice: number;
  currentPrice: number;
  isClosed: boolean;
  sellerEmail: string;
  winnerEmail?: string | null;
  bids: BidDto[];

  // countdown helpers from API
  remainingSeconds: number;
  serverTimeUtc: string;   // ISO
}
