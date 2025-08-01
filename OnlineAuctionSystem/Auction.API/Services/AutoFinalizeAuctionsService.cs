using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Auction.API.Data;
using AuctionApp.Models;

namespace Auction.API.Services
{
    /// <summary>
    /// Background worker that periodically closes expired auctions and persists the winner + final price.
    /// </summary>
    public class AutoFinalizeAuctionsService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AutoFinalizeAuctionsService> _logger;
        private readonly TimeSpan _pollInterval;

        // You can make the poll interval configurable via appsettings if you like
        public AutoFinalizeAuctionsService(
            IServiceScopeFactory scopeFactory,
            ILogger<AutoFinalizeAuctionsService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _pollInterval = TimeSpan.FromSeconds(15); // adjust to taste
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutoFinalizeAuctionsService started. Poll interval: {interval}", _pollInterval);

            // Periodically check for auctions to finalize until shutdown
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FinalizeExpiredAuctionsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // expected on shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while finalizing expired auctions");
                }

                try
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // shutting down
                }
            }

            _logger.LogInformation("AutoFinalizeAuctionsService stopping.");
        }

        private async Task FinalizeExpiredAuctionsAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Find auctions that passed EndTime and are not closed yet
            var dueAuctions = await db.Auctions
                .Include(a => a.Bids).ThenInclude(b => b.Bidder)
                .Where(a => !a.IsClosed && a.EndTime <= now)
                .ToListAsync(ct);

            if (dueAuctions.Count == 0)
                return;

            foreach (var a in dueAuctions)
            {
                // Mark closed
                a.IsClosed = true;

                // Winner = highest bid by amount (if any); final price = that amount or StartPrice
                var topBid = a.Bids
                    .OrderByDescending(b => b.Amount)
                    .FirstOrDefault();

                a.WinnerId = topBid?.BidderId;
                a.CurrentPrice = topBid?.Amount ?? a.StartPrice;

                _logger.LogInformation(
                    "Finalized auction {AuctionId}. WinnerId={WinnerId}, FinalPrice={FinalPrice}",
                    a.Id, a.WinnerId, a.CurrentPrice);
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
