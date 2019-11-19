using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init28 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Detail",
                table: "porsche_storeactivity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerDetail",
                table: "porsche_activity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MobileImgs",
                table: "porsche_activity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IntroDetail",
                table: "porsche_activity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Imgs",
                table: "porsche_activity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EPerDetail",
                table: "porsche_activity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EIntroDetail",
                table: "porsche_activity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EBackDetail",
                table: "porsche_activity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BackDetail",
                table: "porsche_activity",
                maxLength: 40000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Detail",
                table: "porsche_storeactivity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MobileImgs",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IntroDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Imgs",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EPerDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EIntroDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EBackDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BackDetail",
                table: "porsche_activity",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40000,
                oldNullable: true);
        }
    }
}
