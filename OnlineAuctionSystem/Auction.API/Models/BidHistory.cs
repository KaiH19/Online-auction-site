using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auction.API.Models
{
    public class BidHistory
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public required string UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public AuctionApp.Models.Auction Auction { get; set; } // fully qualified
        public AuctionApp.Models.User User { get; set; }       // fully qualified
    }
}
