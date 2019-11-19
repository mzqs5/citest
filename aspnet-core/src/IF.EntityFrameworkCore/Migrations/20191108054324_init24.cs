using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init24 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Option",
                table: "porsche_activity",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Option",
                table: "porsche_activity");
        }
    }
}
