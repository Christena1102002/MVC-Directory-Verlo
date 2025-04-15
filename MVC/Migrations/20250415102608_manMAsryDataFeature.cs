using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 
namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class manMAsryDataFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Packages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PackageFeature",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsIncluded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageFeature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageFeature_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "Id", "Description", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[,]
                {
                    { 1, "Basic free package for small businesses", 0m, "Basic Package", 0m },
                    { 2, "Comprehensive package for medium businesses with additional features", 19.99m, "Premium Package", 199.99m },
                    { 3, "Complete package for large businesses with all available features", 49.99m, "Enterprise Package", 499.99m }
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
                    { 11, "View visits reports and statistics", false, 1 },
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
                    { 26, "View visits reports and statistics", true, 2 },
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
                    { 41, "View visits reports and statistics", true, 3 },
                    { 42, "Add offers and discounts", true, 3 },
                    { 43, "24/7 technical support", true, 3 },
                    { 44, "Direct booking capability", true, 3 },
                    { 45, "Create online store", true, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageFeature_PackageId",
                table: "PackageFeature",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageFeature");

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

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Packages");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
