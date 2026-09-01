using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationUniqueIndexesIgnoreDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organizations_Title",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Voen",
                table: "Organizations");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Title",
                table: "Organizations",
                column: "Title",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Voen",
                table: "Organizations",
                column: "Voen",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organizations_Title",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Voen",
                table: "Organizations");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Title",
                table: "Organizations",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Voen",
                table: "Organizations",
                column: "Voen",
                unique: true);
        }
    }
}
