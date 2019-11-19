using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Store");

            migrationBuilder.DropColumn(
                name: "System_PlaceId",
                table: "Store");

            migrationBuilder.DropColumn(
                name: "BgPic",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Pic",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Store",
                newName: "More");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Store",
                newName: "EPhone");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Store",
                newName: "EName");

            migrationBuilder.RenameColumn(
                name: "DDesc",
                table: "Store",
                newName: "EMore");

            migrationBuilder.RenameColumn(
                name: "AUserId",
                table: "Store",
                newName: "EDate");

            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "Store",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EAddress",
                table: "Store",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Store");

            migrationBuilder.DropColumn(
                name: "EAddress",
                table: "Store");

            migrationBuilder.RenameColumn(
                name: "More",
                table: "Store",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "EPhone",
                table: "Store",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "EName",
                table: "Store",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "EMore",
                table: "Store",
                newName: "DDesc");

            migrationBuilder.RenameColumn(
                name: "EDate",
                table: "Store",
                newName: "AUserId");

            migrationBuilder.AddColumn<float>(
                name: "Distance",
                table: "Store",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "System_PlaceId",
                table: "Store",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BgPic",
                table: "Cars",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pic",
                table: "Cars",
                nullable: true);
        }
    }
}
