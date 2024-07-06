using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingCartService.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "CartItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CartItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "CartItems",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CartItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "CartItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CartItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "CartItems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
