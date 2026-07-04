using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceFinderAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackedProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    StoreName = table.Column<string>(type: "TEXT", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackedProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Query = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedProducts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceSnapshots_TrackedProductId_CheckedAt",
                table: "PriceSnapshots",
                columns: new[] { "TrackedProductId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackedProducts_Query",
                table: "TrackedProducts",
                column: "Query",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceSnapshots");

            migrationBuilder.DropTable(
                name: "TrackedProducts");
        }
    }
}
