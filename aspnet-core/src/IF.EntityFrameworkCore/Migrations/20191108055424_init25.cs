using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init25 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EOption",
                table: "porsche_activity",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EOption",
                table: "porsche_activity");
        }
    }
}
