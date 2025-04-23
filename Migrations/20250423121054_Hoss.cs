using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class Hoss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "PackageFeature",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "Id", "Description", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[,]
                {
                    { 1, "Free basic package for small businesses", 0m, "Basic Package", 0m },
                    { 2, "Comprehensive package for medium businesses with additional features", 19.99m, "Premium Package", 199.99m },
                    { 3, "Complete package for large businesses with all available features", 49.99m, "Advanced Package", 499.99m }
                });

            migrationBuilder.InsertData(
                table: "PackageFeature",
                columns: new[] { "Id", "Description", "IsIncluded", "PackageId" },
                values: new object[,]
                {
                    { 1, "Add one business", true, 1 },
                    { 2, "Display basic business information", true, 1 },
                    { 3, "Add one main image", true, 1 },
                    { 4, "Appear in search results", true, 1 },
                    { 5, "Add basic contact information", true, 1 },
                    { 6, "Add business hours", true, 1 },
                    { 7, "Email technical support", true, 1 },
                    { 8, "Add multiple images gallery", false, 1 },
                    { 9, "Featured listings", false, 1 },
                    { 10, "Social media promotion", false, 1 },
                    { 11, "View visits statistics and reports", false, 1 },
                    { 12, "Add offers and discounts", false, 1 },
                    { 13, "24/7 technical support", false, 1 },
                    { 14, "Direct booking capability", false, 1 },
                    { 15, "Create online store", false, 1 },
                    { 16, "Add one business", true, 2 },
                    { 17, "Display basic business information", true, 2 },
                    { 18, "Add one main image", true, 2 },
                    { 19, "Appear in search results", true, 2 },
                    { 20, "Add basic contact information", true, 2 },
                    { 21, "Add business hours", true, 2 },
                    { 22, "Email technical support", true, 2 },
                    { 23, "Add multiple images gallery (up to 15 images)", true, 2 },
                    { 24, "Featured listings", true, 2 },
                    { 25, "Social media promotion", true, 2 },
                    { 26, "View visits statistics and reports", true, 2 },
                    { 27, "Add offers and discounts", false, 2 },
                    { 28, "24/7 technical support", false, 2 },
                    { 29, "Direct booking capability", false, 2 },
                    { 30, "Create online store", false, 2 },
                    { 31, "Add one business", true, 3 },
                    { 32, "Display basic business information", true, 3 },
                    { 33, "Add one main image", true, 3 },
                    { 34, "Appear in search results", true, 3 },
                    { 35, "Add basic contact information", true, 3 },
                    { 36, "Add business hours", true, 3 },
                    { 37, "Email technical support", true, 3 },
                    { 38, "Add multiple images gallery (unlimited)", true, 3 },
                    { 39, "Featured listings", true, 3 },
                    { 40, "Social media promotion", true, 3 },
                    { 41, "View visits statistics and reports", true, 3 },
                    { 42, "Add offers and discounts", true, 3 },
                    { 43, "24/7 technical support", true, 3 },
                    { 44, "Direct booking capability", true, 3 },
                    { 45, "Create online store", true, 3 }
                });
        }
    }
}
