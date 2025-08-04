using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionApp.Models
{
    public class Auction
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public decimal StartPrice { get; set; }

        public decimal? CurrentPrice { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime EndTime { get; set; }

        public bool IsClosed { get; set; } = false;

        [Required]
        public string SellerId { get; set; }
        public User Seller { get; set; }

        public string? WinnerId { get; set; }
        public User? Winner { get; set; }

        public List<Bid> Bids { get; set; } = new();
        public string? WinnerEmail { get; set; }

        public string? Status { get; set; } // e.g., "AwaitingPayment", "Paid", "NoBids"
    }
}

