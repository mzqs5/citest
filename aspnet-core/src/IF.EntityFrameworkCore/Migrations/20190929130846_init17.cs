using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "porsche_storeactivity",
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
                    Title = table.Column<string>(nullable: true),
                    StoreId = table.Column<int>(nullable: false),
                    Desc = table.Column<string>(maxLength: 500, nullable: true),
                    ImgUrl = table.Column<string>(nullable: true),
                    Detail = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_porsche_storeactivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_porsche_storeactivity_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_storeactivity_AbpUsers_DeleterUserId",
                        column: x => x.DeleterUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_storeactivity_AbpUsers_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_porsche_storeactivity_CreatorUserId",
                table: "porsche_storeactivity",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_storeactivity_DeleterUserId",
                table: "porsche_storeactivity",
                column: "DeleterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_storeactivity_LastModifierUserId",
                table: "porsche_storeactivity",
                column: "LastModifierUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "porsche_storeactivity");
        }
    }
}
