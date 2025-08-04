using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auction.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuctionWinnerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Auctions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WinnerEmail",
                table: "Auctions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "WinnerEmail",
                table: "Auctions");
        }
    }
}
