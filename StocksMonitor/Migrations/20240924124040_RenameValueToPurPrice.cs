using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksMonitor.Migrations
{
    /// <inheritdoc />
    public partial class RenameValueToPurPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "stockData",
                newName: "PurPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurPrice",
                table: "stockData",
                newName: "Value");
        }
    }
}
