using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPNET_Ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class Phase18HomepageSliderAndProductDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowHomepageCategories",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowHomepageDiscountProducts",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowHomepageFeaturedProducts",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowHomepageNewProducts",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowHomepageSlider",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Products",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiscountActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SliderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrimaryButtonUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    SecondaryButtonUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    BackgroundImagePath = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    ClickUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SliderItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SliderItems_IsActive_DisplayOrder",
                table: "SliderItems",
                columns: new[] { "IsActive", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SliderItems");

            migrationBuilder.DropColumn(
                name: "ShowHomepageCategories",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ShowHomepageDiscountProducts",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ShowHomepageFeaturedProducts",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ShowHomepageNewProducts",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ShowHomepageSlider",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDiscountActive",
                table: "Products");
        }
    }
}
