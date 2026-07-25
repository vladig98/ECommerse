using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingCartService.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingCartItemQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CartItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CartItems");
        }
    }
}
