using Microsoft.EntityFrameworkCore.Migrations;

namespace BrowserVersions.Data.Migrations
{
    public partial class Add_ReleaseChannel_ToVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReleaseChannel",
                table: "Versions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseChannel",
                table: "Versions");
        }
    }
}
