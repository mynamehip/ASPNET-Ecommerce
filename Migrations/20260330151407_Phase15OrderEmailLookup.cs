using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPNET_Ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class Phase15OrderEmailLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "SystemSettings",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "SystemSettings");
        }
    }
}
