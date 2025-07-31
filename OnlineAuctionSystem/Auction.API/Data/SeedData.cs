using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// aliases to match your setup
using AppUser     = AuctionApp.Models.User;
using AuctionItem = AuctionApp.Models.Auction;
using BidItem     = AuctionApp.Models.Bid;

namespace Auction.API.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var db          = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            // Ensure DB is up to date
            await db.Database.MigrateAsync();

            // ---- Users ----
            var seller = await userManager.FindByNameAsync("seller1");
            if (seller == null)
            {
                seller = new AppUser
                {
                    UserName = "seller1",
                    Email = "seller1@example.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(seller, "Seller1!Pass");
            }

            var bidder = await userManager.FindByNameAsync("bidder1");
            if (bidder == null)
            {
                bidder = new AppUser
                {
                    UserName = "bidder1",
                    Email = "bidder1@example.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(bidder, "Bidder1!Pass");
            }

            // ---- Auction & Bid ----
            if (!await db.Auctions.AnyAsync())
            {
                var auction = new AuctionItem
                {
                    Title = "Vintage Watch",
                    Description = "Classic Omega watch from the 1950s",
                    StartPrice = 500m,
                    CurrentPrice = 650m,
                    StartTime = new DateTime(2025, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2025, 8, 4, 12, 0, 0, DateTimeKind.Utc),
                    IsClosed = false,
                    SellerId = seller.Id
                };
                db.Auctions.Add(auction);
                await db.SaveChangesAsync();

                db.Bids.Add(new BidItem
                {
                    Amount = 650m,
                    Timestamp = new DateTime(2025, 8, 1, 14, 0, 0, DateTimeKind.Utc),
                    AuctionId = auction.Id,
                    BidderId = bidder.Id
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
