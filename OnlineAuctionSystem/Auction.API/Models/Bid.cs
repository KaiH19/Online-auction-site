using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionApp.Models
{
    public class Bid
    {
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Foreign key to Auction
        [Required]
        public int AuctionId { get; set; }
        public Auction Auction { get; set; }

        // Foreign key to Bidder (User)
        [Required]
        public string BidderId { get; set; }
        public User Bidder { get; set; }
    }
}
