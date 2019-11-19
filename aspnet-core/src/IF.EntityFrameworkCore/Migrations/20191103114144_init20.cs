using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "porsche_orderuse",
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
                    OrderId = table.Column<int>(nullable: false),
                    ETime = table.Column<DateTime>(nullable: false),
                    GoodsName = table.Column<string>(nullable: true),
                    PayAmount = table.Column<decimal>(nullable: false),
                    Balance = table.Column<decimal>(nullable: false),
                    WorkNo = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    Remarks = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_porsche_orderuse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_porsche_orderuse_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_orderuse_AbpUsers_DeleterUserId",
                        column: x => x.DeleterUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_orderuse_AbpUsers_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_porsche_orderuse_CreatorUserId",
                table: "porsche_orderuse",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_orderuse_DeleterUserId",
                table: "porsche_orderuse",
                column: "DeleterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_orderuse_LastModifierUserId",
                table: "porsche_orderuse",
                column: "LastModifierUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "porsche_orderuse");
        }
    }
}
