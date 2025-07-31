using System.ComponentModel.DataAnnotations;

namespace AuctionApp.Models.DTOs
{
    public class AuctionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public decimal StartPrice { get; set; }
        public decimal? CurrentPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsClosed { get; set; }

        public string SellerEmail { get; set; }
        public string? WinnerEmail { get; set; }

        public List<BidDto> Bids { get; set; }
    }
}
