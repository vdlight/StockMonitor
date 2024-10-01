using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddMiscTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "miscs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsHidden = table.Column<bool>(type: "BIT", nullable: false),
                    HiddenReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StockId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_miscs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_miscs_stockData_StockId",
                        column: x => x.StockId,
                        principalTable: "stockData",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_miscs_StockId",
                table: "miscs",
                column: "StockId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "miscs");
        }
    }
}
