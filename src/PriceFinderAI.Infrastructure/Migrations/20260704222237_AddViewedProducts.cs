using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceFinderAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViewedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewedProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    TrackedProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewedProducts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewedProducts_UserId_TrackedProductId",
                table: "ViewedProducts",
                columns: new[] { "UserId", "TrackedProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewedProducts");
        }
    }
}
