using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class HistoryProjectIndexPartial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HistoryEntries_WorkProjectId_CreatedAt_Id",
                table: "HistoryEntries");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEntries_WorkProjectId_CreatedAt_Id",
                table: "HistoryEntries",
                columns: new[] { "WorkProjectId", "CreatedAt", "Id" },
                descending: new[] { false, true, true },
                filter: "\"EntityType\" IN (1, 2, 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HistoryEntries_WorkProjectId_CreatedAt_Id",
                table: "HistoryEntries");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEntries_WorkProjectId_CreatedAt_Id",
                table: "HistoryEntries",
                columns: new[] { "WorkProjectId", "CreatedAt", "Id" },
                descending: new[] { false, true, true });
        }
    }
}
