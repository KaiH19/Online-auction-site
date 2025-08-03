using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Auction.API.Data;
using Auction.API.Models;

namespace Auction.API.Hubs
{
    //[Authorize] Left out for demo purposes; normally enabled for authenticated SignalR access
    public class BiddingHub : Hub
    {
        private readonly AppDbContext _context;

        public BiddingHub(AppDbContext context)
        {
            _context = context;
        }

        public static string GroupName(int auctionId) => $"auction-{auctionId}";

        public async Task JoinAuction(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(auctionId));
        }

        public async Task LeaveAuction(int auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(auctionId));
        }

        public async Task SendBid(BidHistory bid)
        {
            await _context.BidHistories.AddAsync(bid);
            await _context.SaveChangesAsync();

            var history = await _context.BidHistories
                .Include(b => b.User) // Ensure we load user info
                .Where(b => b.AuctionId == bid.AuctionId)
                .OrderByDescending(b => b.Timestamp)
                .Select(b => new
                {
                    b.Amount,
                    b.Timestamp,
                    Bidder = b.User.UserName
                })
                .ToListAsync();

            await Clients.Group(bid.AuctionId.ToString())
                .SendAsync("ReceiveBidHistory", history);
        }
    }
}
