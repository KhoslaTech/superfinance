using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ASKSource.Migrations
{
    public partial class ASKFirewallModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FirewallEnabled",
                table: "User",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FirewallRule",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    IpFrom = table.Column<string>(maxLength: 45, nullable: false),
                    IpTo = table.Column<string>(maxLength: 45, nullable: true),
                    EntityUrn = table.Column<string>(maxLength: 72, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirewallRule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UK_FirewallRule_EntityUrn_Name",
                table: "FirewallRule",
                columns: new[] { "EntityUrn", "Name" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirewallRule");

            migrationBuilder.DropColumn(
                name: "FirewallEnabled",
                table: "User");
        }
    }
}
