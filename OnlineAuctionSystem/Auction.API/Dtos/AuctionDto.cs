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

        public decimal StartPrice { get; init; }
        public decimal CurrentPrice { get; init; }
        public bool IsClosed { get; init; }

        public string SellerEmail { get; init; } = string.Empty;
        public string? WinnerEmail { get; init; }

        public List<BidDto> Bids { get; init; } = new();

        // NEW: Countdown helpers for the client
        public int RemainingSeconds { get; init; }          // 0 when closed/past end
        public DateTime ServerTimeUtc { get; init; }        // lets client sync timers reliably
    }
}

