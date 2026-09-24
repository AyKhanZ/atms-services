using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkTaskBoardSortIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_PriorityId",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PriorityId_Id",
                table: "Tasks",
                columns: new[] { "PriorityId", "Id" },
                descending: new[] { true, false });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Rank_Id",
                table: "Tasks",
                columns: new[] { "Rank", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_PriorityId_Id",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Rank_Id",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PriorityId",
                table: "Tasks",
                column: "PriorityId");
        }
    }
}
