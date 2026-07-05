using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceFinderAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToPriceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "PriceSnapshots",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "PriceSnapshots",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PriceSnapshots");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "PriceSnapshots");
        }
    }
}
