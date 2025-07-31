using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auction.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHasDataSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Bids",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-bidder-id");

            migrationBuilder.DeleteData(
                table: "Auctions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-seller-id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "seed-bidder-id", 0, "1b1631c2-d220-4211-bc5d-8abfc7d331dd", "bidder1@example.com", false, false, null, "BIDDER1@EXAMPLE.COM", "BIDDER1", "placeholder", null, false, "7e347227-73db-4784-828c-663153c7caaf", false, "bidder1" },
                    { "seed-seller-id", 0, "c9f9e657-3839-4226-bd02-2ab3e51e1b4f", "seller1@example.com", false, false, null, "SELLER1@EXAMPLE.COM", "SELLER1", "placeholder", null, false, "545352a3-8e73-4b6f-a819-dab2234d4022", false, "seller1" }
                });

            migrationBuilder.InsertData(
                table: "Auctions",
                columns: new[] { "Id", "CurrentPrice", "Description", "EndTime", "IsClosed", "SellerId", "StartPrice", "StartTime", "Title", "WinnerId" },
                values: new object[] { 1, 650m, "Classic Omega watch from the 1950s", new DateTime(2025, 8, 3, 20, 59, 47, 812, DateTimeKind.Utc).AddTicks(8855), false, "seed-seller-id", 500m, new DateTime(2025, 7, 31, 20, 59, 47, 812, DateTimeKind.Utc).AddTicks(8704), "Vintage Watch", null });

            migrationBuilder.InsertData(
                table: "Bids",
                columns: new[] { "Id", "Amount", "AuctionId", "BidderId", "Timestamp" },
                values: new object[] { 1, 650m, 1, "seed-bidder-id", new DateTime(2025, 7, 31, 20, 59, 47, 813, DateTimeKind.Utc).AddTicks(293) });
        }
    }
}
