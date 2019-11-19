using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init22 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PayTime",
                table: "porsche_order",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IsCarOwner",
                table: "AbpUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sex",
                table: "AbpUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayTime",
                table: "porsche_order");

            migrationBuilder.DropColumn(
                name: "IsCarOwner",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "AbpUsers");
        }
    }
}
