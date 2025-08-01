using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Auction.API.Hubs
{
    [Authorize] // only authenticated users connect (you can relax if needed)
    public class BiddingHub : Hub
    {
        public static string GroupName(int auctionId) => $"auction-{auctionId}";

        public async Task JoinAuction(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(auctionId));
        }

        public async Task LeaveAuction(int auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(auctionId));
        }
    }
}
