using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Validity",
                table: "porsche_orderitem",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Validity",
                table: "porsche_orderitem");
        }
    }
}
