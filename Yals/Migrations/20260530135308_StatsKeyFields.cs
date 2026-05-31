using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yals.Migrations
{
    /// <inheritdoc />
    public partial class StatsKeyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "HashedStatsKey",
                table: "Urls",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "StatsKeySalt",
                table: "Urls",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashedStatsKey",
                table: "Urls");

            migrationBuilder.DropColumn(
                name: "StatsKeySalt",
                table: "Urls");
        }
    }
}
