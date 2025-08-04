using System.ComponentModel.DataAnnotations;

namespace AuctionApp.Models.DTOs
{
    public class PaymentRequest
    {
        [Required]
        public int AuctionId { get; init; }

        [Required]
        public string UserId { get; init; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; init; }
    }
}
