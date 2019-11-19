using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init13 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CModelIds",
                table: "porsche_orderitem",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CModels",
                table: "porsche_orderitem",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "porsche_appointmenttestdrive",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDoor",
                table: "porsche_appointment",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "porsche_appointment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "porsche_appointment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "porsche_activity",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "porsche_activity",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromName",
                table: "porsche_activity",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CModelIds",
                table: "porsche_orderitem");

            migrationBuilder.DropColumn(
                name: "CModels",
                table: "porsche_orderitem");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "porsche_appointmenttestdrive");

            migrationBuilder.DropColumn(
                name: "IsDoor",
                table: "porsche_appointment");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "porsche_appointment");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "porsche_appointment");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "FromName",
                table: "porsche_activity");
        }
    }
}
