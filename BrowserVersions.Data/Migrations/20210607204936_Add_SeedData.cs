using Microsoft.EntityFrameworkCore.Migrations;

namespace BrowserVersions.Data.Migrations
{
    public partial class Add_SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 1, 1, 1 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 2, 2, 1 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 3, 3, 1 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 4, 1, 2 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 5, 2, 2 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 6, 3, 2 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 7, 1, 4 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 8, 2, 4 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 9, 3, 4 });

            migrationBuilder.InsertData(
                table: "Browsers",
                columns: new[] { "BrowserId", "Platform", "Type" },
                values: new object[] { 10, 1, 3 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Browsers",
                keyColumn: "BrowserId",
                keyValue: 10);
        }
    }
}
