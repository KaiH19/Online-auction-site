using AuctionApp.Models;
using AuctionApp.Models.DTOs;
using Auction.API.Data;
using Auction.API.Hubs;
using Auction.API.Realtime;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;

namespace Auction.API.Controllers
{
    using AuctionModel = AuctionApp.Models.Auction;

    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<BiddingHub> _hub;

        public AuctionsController(AppDbContext context, IHubContext<BiddingHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        /// <summary>
        /// Get all auctions with related bids, seller and winner info.
        /// Also finalizes auctions that have timed out (sets IsClosed, WinnerId, CurrentPrice),
        /// then broadcasts AuctionClosed to subscribers.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AuctionDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var auctions = await _context.Auctions
                .Include(a => a.Bids).ThenInclude(b => b.Bidder)
                .Include(a => a.Seller)
                .Include(a => a.Winner)
                .ToListAsync();

            var now = DateTime.UtcNow;

            // Track expired auctions to broadcast after persistence
            var closedToBroadcast = new List<(int AuctionId, decimal FinalPrice, string? WinnerEmail)>();
            bool changed = false;

            foreach (var auction in auctions)
            {
                if (!auction.IsClosed && now > auction.EndTime)
                {
                    auction.IsClosed = true;

                    var topBid = auction.Bids
                        .OrderByDescending(b => b.Amount)
                        .FirstOrDefault();

                    // Ensure non-null final price
                    var finalPrice = topBid?.Amount ?? auction.StartPrice;
                    var winnerEmail = topBid?.Bidder?.Email ?? auction.Winner?.Email;

                    auction.WinnerId = topBid?.BidderId;
                    auction.CurrentPrice = finalPrice;

                    closedToBroadcast.Add((auction.Id, finalPrice, winnerEmail));
                    changed = true;
                }
            }

            if (changed)
            {
                await _context.SaveChangesAsync();

                // Broadcast after persistence
                foreach (var x in closedToBroadcast)
                {
                    await _hub.Clients
                        .Group(BiddingHub.GroupName(x.AuctionId))
                        .SendAsync("AuctionClosed", new AuctionClosedEvent
                        {
                            AuctionId = x.AuctionId,
                            FinalPrice = x.FinalPrice,
                            WinnerEmail = x.WinnerEmail,
                            ClosedAt = DateTime.UtcNow.ToString("o")
                        });
                }
            }

            var result = auctions.Select(MapToDto).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Get a specific auction by ID.
        /// Also finalizes it if it has timed out, then broadcasts AuctionClosed.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AuctionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var auction = await _context.Auctions
                .Include(a => a.Bids).ThenInclude(b => b.Bidder)
                .Include(a => a.Seller)
                .Include(a => a.Winner)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
                return NotFound("Auction not found.");

            var now = DateTime.UtcNow;
            if (!auction.IsClosed && now > auction.EndTime)
            {
                auction.IsClosed = true;

                var topBid = auction.Bids
                    .OrderByDescending(b => b.Amount)
                    .FirstOrDefault();

                var finalPrice = topBid?.Amount ?? auction.StartPrice;
                var winnerEmail = topBid?.Bidder?.Email ?? auction.Winner?.Email;

                auction.WinnerId = topBid?.BidderId;
                auction.CurrentPrice = finalPrice;

                await _context.SaveChangesAsync();

                await _hub.Clients
                    .Group(BiddingHub.GroupName(auction.Id))
                    .SendAsync("AuctionClosed", new AuctionClosedEvent
                    {
                        AuctionId = auction.Id,
                        FinalPrice = finalPrice,
                        WinnerEmail = winnerEmail,
                        ClosedAt = DateTime.UtcNow.ToString("o")
                    });
            }

            return Ok(MapToDto(auction));
        }

        /// <summary>
        /// Create a new auction. Admin or Seller only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> CreateAuction([FromBody] AuctionModel auction)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            auction.SellerId = userId;
            auction.CurrentPrice = auction.StartPrice;
            auction.IsClosed = false;

            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = auction.Id }, MapToDto(auction));
        }

        /// <summary>
        /// Update auction details. Only before auction starts.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> UpdateAuction(int id, [FromBody] AuctionModel updatedAuction)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound("Auction not found.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (auction.SellerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (DateTime.UtcNow >= auction.StartTime)
                return BadRequest("Cannot update an auction after it has started.");

            auction.Title = updatedAuction.Title;
            auction.Description = updatedAuction.Description;
            auction.StartPrice = updatedAuction.StartPrice;
            auction.CurrentPrice = updatedAuction.StartPrice;
            auction.StartTime = updatedAuction.StartTime;
            auction.EndTime = updatedAuction.EndTime;

            await _context.SaveChangesAsync();
            return Ok(MapToDto(auction));
        }

        /// <summary>
        /// Delete an auction. Only if no bids placed.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> DeleteAuction(int id)
        {
            var auction = await _context.Auctions
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null) return NotFound("Auction not found.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (auction.SellerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (auction.Bids.Any())
                return BadRequest("Cannot delete an auction that already has bids.");

            _context.Auctions.Remove(auction);
            await _context.SaveChangesAsync();

            return Ok($"Auction {id} deleted.");
        }

        /// <summary>
        /// Place a bid on an open auction (broadcasts BidPlaced).
        /// </summary>
        [HttpPost("{id:int}/bids")]
        [Authorize]
        public async Task<IActionResult> PlaceBid(int id, [FromBody] decimal amount)
        {
            var auction = await _context.Auctions
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null) return NotFound("Auction not found.");

            if (auction.IsClosed || DateTime.UtcNow > auction.EndTime)
            {
                auction.IsClosed = true;
                await _context.SaveChangesAsync();

                // Notify clients it has closed (edge case)
                await _hub.Clients
                    .Group(BiddingHub.GroupName(id))
                    .SendAsync("AuctionClosed", new AuctionClosedEvent
                    {
                        AuctionId = id,
                        FinalPrice = auction.CurrentPrice ?? auction.StartPrice,
                        WinnerEmail = null,
                        ClosedAt = DateTime.UtcNow.ToString("o")
                    });

                return BadRequest("Auction is closed.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            if (auction.SellerId == userId)
                return BadRequest("You cannot bid on your own auction.");

            if (amount <= auction.CurrentPrice)
                return BadRequest($"Bid must be higher than current price: {auction.CurrentPrice}");

            var bid = new Bid
            {
                AuctionId = id,
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                BidderId = userId
            };

            auction.Bids.Add(bid);
            auction.CurrentPrice = amount;
            auction.WinnerId = userId;

            await _context.SaveChangesAsync();

            var bidderEmail = (await _context.Users.FindAsync(userId))?.Email ?? string.Empty;

            await _hub.Clients
                .Group(BiddingHub.GroupName(id))
                .SendAsync("BidPlaced", new BidPlacedEvent
                {
                    AuctionId = id,
                    BidId = bid.Id,
                    Amount = bid.Amount,
                    CurrentPrice = auction.CurrentPrice ?? auction.StartPrice,
                    BidderEmail = bidderEmail,
                    Timestamp = bid.Timestamp.ToUniversalTime().ToString("o")
                });

            return Ok(new BidDto
            {
                Id = bid.Id,
                Amount = bid.Amount,
                Timestamp = bid.Timestamp,
                BidderEmail = bidderEmail
            });
        }

        /// <summary>
        /// Map Auction entity to AuctionDto, including countdown helpers.
        /// </summary>
        private static AuctionDto MapToDto(AuctionModel a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));

            var now = DateTime.UtcNow;

            // Current price = latest bid by time, or StartPrice if none
            var currentPrice = a.Bids
                .OrderByDescending(b => b.Timestamp)
                .Select(b => b.Amount)
                .DefaultIfEmpty(a.StartPrice)
                .First();

            // Winner email: prefer Winner nav, else highest bid's bidder
            var winnerEmail = a.Winner?.Email
                ?? a.Bids
                    .OrderByDescending(b => b.Amount)
                    .Select(b => b.Bidder != null ? b.Bidder.Email : null)
                    .FirstOrDefault();

            var isClosed = a.IsClosed || now >= a.EndTime;
            var remainingSeconds = isClosed
                ? 0
                : Math.Max(0, (int)(a.EndTime - now).TotalSeconds);

            return new AuctionDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                StartTime = a.StartTime,
                EndTime = a.EndTime,

                StartPrice = a.StartPrice,
                CurrentPrice = currentPrice,
                IsClosed = isClosed,

                SellerEmail = a.Seller?.Email ?? string.Empty,
                WinnerEmail = winnerEmail,

                Bids = a.Bids
                    .OrderByDescending(b => b.Timestamp)
                    .Select(b => new BidDto
                    {
                        Id = b.Id,
                        Amount = b.Amount,
                        Timestamp = b.Timestamp,
                        BidderEmail = b.Bidder?.Email ?? string.Empty
                    })
                    .ToList(),

                // Countdown helpers
                RemainingSeconds = remainingSeconds,
                ServerTimeUtc = now
            };
        }
    }
}

