using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "porsche_appointmentactivity");

            migrationBuilder.AlterColumn<int>(
                name: "ActivityId",
                table: "porsche_appointmentactivity",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sex",
                table: "porsche_appointmentactivity",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "porsche_appointmentactivity",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sex",
                table: "porsche_appointmentactivity");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "porsche_appointmentactivity");

            migrationBuilder.AlterColumn<int>(
                name: "ActivityId",
                table: "porsche_appointmentactivity",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "porsche_appointmentactivity",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
