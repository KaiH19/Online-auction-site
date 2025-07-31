using System.ComponentModel.DataAnnotations;

namespace AuctionApp.Models.DTOs
{
    public class BidDto
    {
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string BidderEmail { get; set; }
    }
}
