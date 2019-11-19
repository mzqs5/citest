using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init27 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileImgs",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileThumbnailUrl",
                table: "porsche_activity",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileImgs",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "MobileThumbnailUrl",
                table: "porsche_activity");
        }
    }
}
