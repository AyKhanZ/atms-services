using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkTaskPriorityIndexBothWays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_PriorityId_Id",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PriorityId_Id",
                table: "Tasks",
                columns: new[] { "PriorityId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_PriorityId_Id",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PriorityId_Id",
                table: "Tasks",
                columns: new[] { "PriorityId", "Id" },
                descending: new[] { true, false });
        }
    }
}
