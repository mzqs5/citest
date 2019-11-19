using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EAddress",
                table: "porsche_activity",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EBackDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EDate",
                table: "porsche_activity",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EDesc",
                table: "porsche_activity",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EFromName",
                table: "porsche_activity",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EPerDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ETitle",
                table: "porsche_activity",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EAddress",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "EBackDetail",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "EDate",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "EDesc",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "EFromName",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "EPerDetail",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "ETitle",
                table: "porsche_activity");
        }
    }
}
