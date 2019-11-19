using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EIntroDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntroDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EIntroDetail",
                table: "porsche_activity");

            migrationBuilder.DropColumn(
                name: "IntroDetail",
                table: "porsche_activity");
        }
    }
}
