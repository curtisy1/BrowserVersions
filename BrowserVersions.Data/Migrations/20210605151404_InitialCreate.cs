using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BrowserVersions.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Browsers",
                columns: table => new
                {
                    BrowserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Browsers", x => x.BrowserId);
                });

            migrationBuilder.CreateTable(
                name: "Versions",
                columns: table => new
                {
                    VersionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionCode = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndOfSupportDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Versions", x => x.VersionId);
                });

            migrationBuilder.CreateTable(
                name: "BrowserVersion",
                columns: table => new
                {
                    BrowsersBrowserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionsVersionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrowserVersion", x => new { x.BrowsersBrowserId, x.VersionsVersionId });
                    table.ForeignKey(
                        name: "FK_BrowserVersion_Browsers_BrowsersBrowserId",
                        column: x => x.BrowsersBrowserId,
                        principalTable: "Browsers",
                        principalColumn: "BrowserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BrowserVersion_Versions_VersionsVersionId",
                        column: x => x.VersionsVersionId,
                        principalTable: "Versions",
                        principalColumn: "VersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrowserVersion_VersionsVersionId",
                table: "BrowserVersion",
                column: "VersionsVersionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrowserVersion");

            migrationBuilder.DropTable(
                name: "Browsers");

            migrationBuilder.DropTable(
                name: "Versions");
        }
    }
}
