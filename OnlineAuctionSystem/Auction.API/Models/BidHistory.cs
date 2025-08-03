using System;
using System.ComponentModel.DataAnnotations.Schema;
using Auction.API.Models;          // for Auction entity
using AuctionApp.Models;

namespace Auction.API.Models
{
    public class BidHistory
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public Auction Auction { get; set; } // reference to auction entity
        public User User { get; set; }       // reference to user
    }
}