using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ASKSource.Migrations
{
    public partial class ASKMFAModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MFAValidUntilSessionExpired",
                table: "UserSession",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecentAccessWithMFAAt",
                table: "UserSession",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MFANotRequired",
                table: "User",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserMultiFactor",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMultiFactor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMultiFactor_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMultiFactor_UserId",
                table: "UserMultiFactor",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMultiFactor");

            migrationBuilder.DropColumn(
                name: "MFAValidUntilSessionExpired",
                table: "UserSession");

            migrationBuilder.DropColumn(
                name: "RecentAccessWithMFAAt",
                table: "UserSession");

            migrationBuilder.DropColumn(
                name: "MFANotRequired",
                table: "User");
        }
    }
}
