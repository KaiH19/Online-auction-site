using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using Auction.API.Data;
using Auction.API.Models;
using AuctionApp.Models.DTOs; 
using System.Text.Json;

namespace Auction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public PaymentsController(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] PaymentRequest request)
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(request.Amount * 100), // Stripe expects cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Auction #{request.AuctionId} Winning Bid"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{_config["AppSettings:FrontendUrl"]}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_config["AppSettings:FrontendUrl"]}/payment-cancelled",
                Metadata = new Dictionary<string, string>
                {
                    { "AuctionId", request.AuctionId.ToString() },
                    { "UserId", request.UserId }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new { sessionUrl = session.Url });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _config["Stripe:WebhookSecret"];

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret
                );
            }
            catch (StripeException e)
            {
                return BadRequest($"Webhook error: {e.Message}");
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                var auctionId = int.Parse(session.Metadata["AuctionId"]);

                var auction = await _context.Auctions.FindAsync(auctionId);
                if (auction != null && auction.Status != "Paid")
                {
                    auction.Status = "Paid";
                    await _context.SaveChangesAsync();
                }
            }

            return Ok();
        }
    }
}
