using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPNET_Ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class Phase16AddressStructureRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingAddressLine1",
                table: "Orders",
                newName: "ShippingAddress");

            migrationBuilder.RenameColumn(
                name: "ShippingCity",
                table: "Orders",
                newName: "ShippingWard");

            migrationBuilder.RenameColumn(
                name: "DefaultAddressLine1",
                table: "AspNetUsers",
                newName: "DefaultAddress");

            migrationBuilder.RenameColumn(
                name: "DefaultCity",
                table: "AspNetUsers",
                newName: "DefaultWard");

            migrationBuilder.Sql(
                @"UPDATE AspNetUsers
                  SET DefaultAddress = CASE
                        WHEN NULLIF(LTRIM(RTRIM(DefaultAddressLine2)), '') IS NULL THEN NULLIF(LTRIM(RTRIM(DefaultAddress)), '')
                        WHEN NULLIF(LTRIM(RTRIM(DefaultAddress)), '') IS NULL THEN LTRIM(RTRIM(DefaultAddressLine2))
                        ELSE LTRIM(RTRIM(DefaultAddress)) + ', ' + LTRIM(RTRIM(DefaultAddressLine2))
                      END,
                      DefaultWard = CASE
                        WHEN NULLIF(LTRIM(RTRIM(DefaultProvince)), '') IS NULL THEN NULL
                        ELSE NULLIF(LTRIM(RTRIM(DefaultWard)), '')
                      END,
                      DefaultProvince = COALESCE(NULLIF(LTRIM(RTRIM(DefaultProvince)), ''), NULLIF(LTRIM(RTRIM(DefaultWard)), ''));");

            migrationBuilder.Sql(
                @"UPDATE Orders
                  SET ShippingAddress = CASE
                        WHEN NULLIF(LTRIM(RTRIM(ShippingAddressLine2)), '') IS NULL THEN LTRIM(RTRIM(ShippingAddress))
                        WHEN NULLIF(LTRIM(RTRIM(ShippingAddress)), '') IS NULL THEN LTRIM(RTRIM(ShippingAddressLine2))
                        ELSE LTRIM(RTRIM(ShippingAddress)) + ', ' + LTRIM(RTRIM(ShippingAddressLine2))
                      END,
                      ShippingWard = CASE
                        WHEN NULLIF(LTRIM(RTRIM(ShippingProvince)), '') IS NULL THEN N''
                        ELSE LTRIM(RTRIM(ShippingWard))
                      END,
                      ShippingProvince = COALESCE(NULLIF(LTRIM(RTRIM(ShippingProvince)), ''), NULLIF(LTRIM(RTRIM(ShippingWard)), ''), N'');");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddressLine2",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DefaultAddressLine2",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultPostalCode",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingProvince",
                table: "Orders",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShippingProvince",
                table: "Orders",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddressLine2",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultAddressLine2",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultPostalCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE Orders
                  SET ShippingWard = CASE
                        WHEN NULLIF(LTRIM(RTRIM(ShippingWard)), '') IS NULL THEN ShippingProvince
                        ELSE ShippingWard
                      END,
                      ShippingProvince = CASE
                        WHEN NULLIF(LTRIM(RTRIM(ShippingWard)), '') IS NULL OR ShippingProvince = ShippingWard THEN NULL
                        ELSE ShippingProvince
                      END;

                  UPDATE AspNetUsers
                  SET DefaultWard = CASE
                        WHEN NULLIF(LTRIM(RTRIM(DefaultWard)), '') IS NULL THEN DefaultProvince
                        ELSE DefaultWard
                      END,
                      DefaultProvince = CASE
                        WHEN NULLIF(LTRIM(RTRIM(DefaultWard)), '') IS NULL OR DefaultProvince = DefaultWard THEN NULL
                        ELSE DefaultProvince
                      END;");

            migrationBuilder.RenameColumn(
                name: "ShippingWard",
                table: "Orders",
                newName: "ShippingCity");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress",
                table: "Orders",
                newName: "ShippingAddressLine1");

            migrationBuilder.RenameColumn(
                name: "DefaultWard",
                table: "AspNetUsers",
                newName: "DefaultCity");

            migrationBuilder.RenameColumn(
                name: "DefaultAddress",
                table: "AspNetUsers",
                newName: "DefaultAddressLine1");
        }
    }
}
