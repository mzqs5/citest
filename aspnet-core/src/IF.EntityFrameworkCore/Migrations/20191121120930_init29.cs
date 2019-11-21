using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init29 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Availability",
                table: "porsche_racing");

            migrationBuilder.DropColumn(
                name: "EventDates",
                table: "porsche_racing");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "porsche_racing");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "porsche_racing",
                newName: "MobileImgs");

            migrationBuilder.RenameColumn(
                name: "Options",
                table: "porsche_racing",
                newName: "Imgs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MobileImgs",
                table: "porsche_racing",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Imgs",
                table: "porsche_racing",
                newName: "Options");

            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "porsche_racing",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventDates",
                table: "porsche_racing",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "porsche_racing",
                nullable: true);
        }
    }
}
