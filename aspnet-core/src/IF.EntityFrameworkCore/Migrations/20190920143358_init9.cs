using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "secret",
                table: "StoreWxPay",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "secret",
                table: "StoreWxPay");
        }
    }
}
