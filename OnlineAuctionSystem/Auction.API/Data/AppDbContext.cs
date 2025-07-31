using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// Alias your model types to avoid any namespace/type name collisions
using AppUser     = AuctionApp.Models.User;
using AuctionItem = AuctionApp.Models.Auction;
using BidItem     = AuctionApp.Models.Bid;

namespace Auction.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<AuctionItem> Auctions { get; set; } = default!;
        public DbSet<BidItem> Bids { get; set; } = default!;
    }

}
