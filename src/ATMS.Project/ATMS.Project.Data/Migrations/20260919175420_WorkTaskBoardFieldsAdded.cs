using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkTaskBoardFieldsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_StatusId",
                table: "Tasks");

            migrationBuilder.AddColumn<Guid>(
                name: "DeletionId",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DoneAt",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rank",
                table: "Tasks",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                collation: "C");

            // Existing tasks get the board order they had nowhere to come from: newest on top,
            // the way new tasks will arrive. The keys are fixed width and end in 'v' rather than
            // '0', so a new card can always be fitted between any two of them.
            migrationBuilder.Sql("""
                UPDATE "Tasks" AS t
                SET "Rank" = ranked."Rank"
                FROM (
                    SELECT "Id",
                           'm' || lpad(row_number() OVER (ORDER BY "CreatedAt" DESC, "Id")::text, 8, '0') || 'v' AS "Rank"
                    FROM "Tasks"
                ) AS ranked
                WHERE ranked."Id" = t."Id";
                """);

            // Tasks already done were closed at their last change, as near as the data can tell.
            migrationBuilder.Sql("""
                UPDATE "Tasks"
                SET "DoneAt" = COALESCE("UpdatedAt", "CreatedAt")
                WHERE "StatusId" = 3;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Deadline_Id",
                table: "Tasks",
                columns: new[] { "Deadline", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DeletionId",
                table: "Tasks",
                column: "DeletionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusId_DoneAt_Id",
                table: "Tasks",
                columns: new[] { "StatusId", "DoneAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusId_Rank_Id",
                table: "Tasks",
                columns: new[] { "StatusId", "Rank", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_Deadline_Id",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_DeletionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_StatusId_DoneAt_Id",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_StatusId_Rank_Id",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DeletionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DoneAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusId",
                table: "Tasks",
                column: "StatusId");
        }
    }
}
