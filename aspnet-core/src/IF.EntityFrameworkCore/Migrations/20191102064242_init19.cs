using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "porsche_order",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Progress",
                table: "porsche_order",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContactState",
                table: "porsche_appointmenttestdrive",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContactState",
                table: "porsche_appointmentactivity",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "porsche_appointmentactivity",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "porsche_appointmentactivity",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContactState",
                table: "porsche_appointment",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "porsche_order");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "porsche_order");

            migrationBuilder.DropColumn(
                name: "ContactState",
                table: "porsche_appointmenttestdrive");

            migrationBuilder.DropColumn(
                name: "ContactState",
                table: "porsche_appointmentactivity");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "porsche_appointmentactivity");

            migrationBuilder.DropColumn(
                name: "State",
                table: "porsche_appointmentactivity");

            migrationBuilder.DropColumn(
                name: "ContactState",
                table: "porsche_appointment");
        }
    }
}
