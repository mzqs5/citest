using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "porsche_orderitem");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "porsche_order");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "porsche_shoppingcart",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "DealerId",
                table: "porsche_order",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "porsche_order");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "porsche_shoppingcart",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "porsche_orderitem",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "porsche_order",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
