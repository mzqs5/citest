using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init26 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileImgUrl",
                table: "porsche_activity",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "porsche_activity",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileImgUrl",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "porsche_activity");
        }
    }
}
