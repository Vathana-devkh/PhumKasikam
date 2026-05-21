using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhumKasikam.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToCrop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Crops",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Crops");
        }
    }
}
