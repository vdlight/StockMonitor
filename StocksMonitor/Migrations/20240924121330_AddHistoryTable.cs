using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "history",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MA200 = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history", x => x.ID);
                    table.ForeignKey(
                        name: "FK_history_stockData_StockId",
                        column: x => x.StockId,
                        principalTable: "stockData",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_history_StockId",
                table: "history",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "history");
        }
    }
}
