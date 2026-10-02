using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class CommentDeletedPlaceholders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_OwnerType_OwnerId_CreatedAt_Id",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_OwnerType_OwnerId_CreatedAt_Id",
                table: "Comments",
                columns: new[] { "OwnerType", "OwnerId", "CreatedAt", "Id" },
                descending: new[] { false, false, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_OwnerType_OwnerId_CreatedAt_Id",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_OwnerType_OwnerId_CreatedAt_Id",
                table: "Comments",
                columns: new[] { "OwnerType", "OwnerId", "CreatedAt", "Id" },
                descending: new[] { false, false, true, true },
                filter: "\"IsDeleted\" = false");
        }
    }
}
