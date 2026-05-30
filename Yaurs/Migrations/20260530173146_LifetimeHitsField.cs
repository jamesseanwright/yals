using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yaurs.Migrations
{
    /// <inheritdoc />
    public partial class LifetimeHitsField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LifetimeHits",
                table: "Urls",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LifetimeHits",
                table: "Urls");
        }
    }
}
