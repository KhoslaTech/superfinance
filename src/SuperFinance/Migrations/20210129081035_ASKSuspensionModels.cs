using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ASKSource.Migrations
{
    public partial class ASKSuspensionModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuspendedEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EntityId = table.Column<Guid>(nullable: false),
                    EntityType = table.Column<string>(maxLength: 30, nullable: false),
                    SuspensionType = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspendedEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuspensionExclusionRule",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EntityTypePattern = table.Column<string>(maxLength: 128, nullable: false),
                    SuspensionTypePattern = table.Column<string>(maxLength: 128, nullable: false),
                    VerbPattern = table.Column<string>(maxLength: 30, nullable: false),
                    OperationPattern = table.Column<string>(maxLength: 512, nullable: false),
                    PossessesAnyOfThePermissions = table.Column<string>(maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspensionExclusionRule", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuspendedEntity");

            migrationBuilder.DropTable(
                name: "SuspensionExclusionRule");
        }
    }
}
