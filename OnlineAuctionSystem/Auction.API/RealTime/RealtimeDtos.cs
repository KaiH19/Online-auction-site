namespace Auction.API.Realtime
{
    public class BidPlacedEvent
    {
        public int AuctionId { get; set; }
        public int BidId { get; set; }
        public decimal Amount { get; set; }
        public string BidderEmail { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public string Timestamp { get; set; } = string.Empty; // ISO
    }

    public class AuctionClosedEvent
    {
        public int AuctionId { get; set; }
        public decimal FinalPrice { get; set; }
        public string? WinnerEmail { get; set; }
        public string ClosedAt { get; set; } = string.Empty; // ISO
    }
}
