using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddedStockList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "List",
                table: "stockData",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "List",
                table: "stockData");
        }
    }
}
