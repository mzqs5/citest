using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "porsche_appointmenttestdrive",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "porsche_appointment",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "porsche_appointment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "porsche_appointment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailAddress",
                table: "porsche_appointment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "porsche_appointment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sort",
                table: "porsche_activitycategory",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sort",
                table: "porsche_activity",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Goods",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatorUserId = table.Column<long>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goods_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goods_AbpUsers_DeleterUserId",
                        column: x => x.DeleterUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goods_AbpUsers_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "porsche_address",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(nullable: false),
                    City = table.Column<string>(nullable: true),
                    Area = table.Column<string>(nullable: true),
                    Street = table.Column<string>(nullable: true),
                    DetailAddress = table.Column<string>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_porsche_address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_porsche_address_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "porsche_appointmentsms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatorUserId = table.Column<long>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    IsAlreadyRead = table.Column<bool>(nullable: false),
                    Msg = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_porsche_appointmentsms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_porsche_appointmentsms_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_appointmentsms_AbpUsers_DeleterUserId",
                        column: x => x.DeleterUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_appointmentsms_AbpUsers_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goods_CreatorUserId",
                table: "Goods",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Goods_DeleterUserId",
                table: "Goods",
                column: "DeleterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Goods_LastModifierUserId",
                table: "Goods",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_address_UserId",
                table: "porsche_address",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_appointmentsms_CreatorUserId",
                table: "porsche_appointmentsms",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_appointmentsms_DeleterUserId",
                table: "porsche_appointmentsms",
                column: "DeleterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_appointmentsms_LastModifierUserId",
                table: "porsche_appointmentsms",
                column: "LastModifierUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Goods");

            migrationBuilder.DropTable(
                name: "porsche_address");

            migrationBuilder.DropTable(
                name: "porsche_appointmentsms");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "porsche_appointment");

            migrationBuilder.DropColumn(
                name: "City",
                table: "porsche_appointment");

            migrationBuilder.DropColumn(
                name: "DetailAddress",
                table: "porsche_appointment");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "porsche_appointment");

            migrationBuilder.DropColumn(
                name: "Sort",
                table: "porsche_activitycategory");

            migrationBuilder.DropColumn(
                name: "Sort",
                table: "porsche_activity");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "porsche_appointmenttestdrive",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "porsche_appointment",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
