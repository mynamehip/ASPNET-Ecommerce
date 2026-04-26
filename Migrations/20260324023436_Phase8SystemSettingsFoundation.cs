using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPNET_Ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class Phase8SystemSettingsFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SupportEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LogoImagePath = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    HeroBadgeText = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    HeroTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeroSubtitle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    HeroPrimaryButtonText = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    HeroPrimaryButtonUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    HeroSecondaryButtonText = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    HeroSecondaryButtonUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    IsPromoBannerActive = table.Column<bool>(type: "bit", nullable: false),
                    PromoTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    PromoSubtitle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PromoButtonText = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PromoButtonUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    PromoCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PromoDiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
