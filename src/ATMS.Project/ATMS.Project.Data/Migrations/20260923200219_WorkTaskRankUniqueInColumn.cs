using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkTaskRankUniqueInColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_StatusId_Rank_Id",
                table: "Tasks");

            // Cards that already share a place in a column (two saved at the same moment) are told apart
            // before the index forbids it: every one after the first gets a suffix, which sorts it right
            // after its twin and keeps the column's order.
            migrationBuilder.Sql("""
                UPDATE "Tasks" AS t
                SET "Rank" = left(t."Rank", 58) || lpad(twins.n::text, 6, '0')
                FROM (
                    SELECT "Id", row_number() OVER (PARTITION BY "StatusId", "Rank" ORDER BY "CreatedAt", "Id") AS n
                    FROM "Tasks"
                    WHERE NOT "IsDeleted" AND "StatusId" <> 3
                ) AS twins
                WHERE twins."Id" = t."Id" AND twins.n > 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusId_Rank",
                table: "Tasks",
                columns: new[] { "StatusId", "Rank" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"StatusId\" <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_StatusId_Rank",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusId_Rank_Id",
                table: "Tasks",
                columns: new[] { "StatusId", "Rank", "Id" });
        }
    }
}
