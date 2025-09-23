using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAuthorizedPersonFromShippingAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizedPersonDni",
                table: "ShippingAddresses");

            migrationBuilder.DropColumn(
                name: "AuthorizedPersonFirstName",
                table: "ShippingAddresses");

            migrationBuilder.DropColumn(
                name: "AuthorizedPersonLastName",
                table: "ShippingAddresses");

            migrationBuilder.DropColumn(
                name: "AuthorizedPersonPhone",
                table: "ShippingAddresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPersonDni",
                table: "ShippingAddresses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPersonFirstName",
                table: "ShippingAddresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPersonLastName",
                table: "ShippingAddresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPersonPhone",
                table: "ShippingAddresses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
