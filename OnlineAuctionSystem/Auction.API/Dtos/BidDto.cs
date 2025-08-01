using System;

namespace AuctionApp.Models.DTOs
{
    public class BidDto
    {
        public int Id { get; init; }                    //optional, but handy for lists
        public decimal Amount { get; init; }
        public DateTime Timestamp { get; init; }        //required by controller
        public string BidderEmail { get; init; } = string.Empty;
    }
}


