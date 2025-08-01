using System;
using System.Collections.Generic;

namespace AuctionApp.Models.DTOs
{
    public class AuctionDto
    {
        public int Id { get; init; }

        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }

        public decimal StartPrice { get; init; }        // <-- required by controller
        public decimal CurrentPrice { get; init; }      // <-- required by controller
        public bool IsClosed { get; init; }             // <-- required by controller

        public string SellerEmail { get; init; } = string.Empty;
        public string? WinnerEmail { get; init; }       // <-- required by controller (nullable)

        public List<BidDto> Bids { get; init; } = new(); // initialize collection
    }
}


