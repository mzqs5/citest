using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Detail",
                table: "porsche_activity");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "porsche_order",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayCer",
                table: "porsche_order",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayMode",
                table: "porsche_order",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "porsche_appointmenttestdrive",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "porsche_appointment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BackDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "porsche_activity",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imgs",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayCer",
                table: "porsche_order");

            migrationBuilder.DropColumn(
                name: "PayMode",
                table: "porsche_order");

            migrationBuilder.DropColumn(
                name: "State",
                table: "porsche_appointmenttestdrive");

            migrationBuilder.DropColumn(
                name: "State",
                table: "porsche_appointment");

            migrationBuilder.DropColumn(
                name: "BackDetail",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "Imgs",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "PerDetail",
                table: "porsche_activity");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "porsche_order",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Detail",
                table: "porsche_activity",
                nullable: true);
        }
    }
}
