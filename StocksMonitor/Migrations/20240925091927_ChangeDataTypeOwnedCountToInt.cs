using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksMonitor.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDataTypeOwnedCountToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OwnedCnt",
                table: "history",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OwnedCnt",
                table: "history",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
