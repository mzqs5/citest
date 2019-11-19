using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IF.Migrations
{
    public partial class init23 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "porsche_activitycategory");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "porsche_activity",
                newName: "Type");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "porsche_activity",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "porsche_appointmentwebactivity",
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
                    SurName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Mobile = table.Column<string>(nullable: true),
                    Option = table.Column<string>(nullable: true),
                    ActivityId = table.Column<int>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    ContactState = table.Column<int>(nullable: false),
                    Remarks = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_porsche_appointmentwebactivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_porsche_appointmentwebactivity_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_appointmentwebactivity_AbpUsers_DeleterUserId",
                        column: x => x.DeleterUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_appointmentwebactivity_AbpUsers_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_porsche_appointmentwebactivity_CreatorUserId",
                table: "porsche_appointmentwebactivity",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_appointmentwebactivity_DeleterUserId",
                table: "porsche_appointmentwebactivity",
                column: "DeleterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_appointmentwebactivity_LastModifierUserId",
                table: "porsche_appointmentwebactivity",
                column: "LastModifierUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "porsche_appointmentwebactivity");

            migrationBuilder.DropColumn(
                name: "State",
                table: "porsche_activity");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "porsche_activity",
                newName: "CategoryId");

            migrationBuilder.CreateTable(
                name: "porsche_activitycategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Sort = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_porsche_activitycategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_porsche_activitycategory_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_activitycategory_AbpUsers_DeleterUserId",
                        column: x => x.DeleterUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_porsche_activitycategory_AbpUsers_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_porsche_activitycategory_CreatorUserId",
                table: "porsche_activitycategory",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_activitycategory_DeleterUserId",
                table: "porsche_activitycategory",
                column: "DeleterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_porsche_activitycategory_LastModifierUserId",
                table: "porsche_activitycategory",
                column: "LastModifierUserId");
        }
    }
}
