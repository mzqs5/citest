using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class addwechatauth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "public_appid",
                table: "StoreWxPay",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "public_secret",
                table: "StoreWxPay",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "public_appid",
                table: "StoreWxPay");

            migrationBuilder.DropColumn(
                name: "public_secret",
                table: "StoreWxPay");
        }
    }
}
