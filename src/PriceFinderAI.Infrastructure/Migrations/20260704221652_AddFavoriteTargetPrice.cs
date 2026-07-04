using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceFinderAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteTargetPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TargetPrice",
                table: "Favorites",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetPrice",
                table: "Favorites");
        }
    }
}
