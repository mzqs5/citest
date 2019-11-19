using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CouponETime",
                table: "porsche_orderitem",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ProductPic",
                table: "porsche_orderitem",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponETime",
                table: "porsche_orderitem");

            migrationBuilder.DropColumn(
                name: "ProductPic",
                table: "porsche_orderitem");
        }
    }
}
