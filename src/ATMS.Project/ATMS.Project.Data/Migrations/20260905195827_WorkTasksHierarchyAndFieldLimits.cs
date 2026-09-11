using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkTasksHierarchyAndFieldLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_ParentWorkTaskId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_WorkProjectId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_WorkTicketId",
                table: "Tasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Tasks_Level",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Tasks");

            // The column limit applies to every stored row, soft-deleted ones included,
            // so refuse with a readable message instead of a raw "value too long" error.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Tasks" WHERE length("Title") > 100) THEN
                        RAISE EXCEPTION 'Tasks contains titles longer than 100 characters, including soft-deleted rows. Shorten them before applying this migration.';
                    END IF;

                    IF EXISTS (SELECT 1 FROM "Tasks" WHERE length("Description") > 2000) THEN
                        RAISE EXCEPTION 'Tasks contains descriptions longer than 2000 characters, including soft-deleted rows. Shorten them before applying this migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ParentWorkTaskId_CreatedAt_Id",
                table: "Tasks",
                columns: new[] { "ParentWorkTaskId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkProjectId_CreatedAt_Id",
                table: "Tasks",
                columns: new[] { "WorkProjectId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkTicketId_ParentWorkTaskId_CreatedAt_Id",
                table: "Tasks",
                columns: new[] { "WorkTicketId", "ParentWorkTaskId", "CreatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_ParentWorkTaskId_CreatedAt_Id",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_WorkProjectId_CreatedAt_Id",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_WorkTicketId_ParentWorkTaskId_CreatedAt_Id",
                table: "Tasks");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Level",
                table: "Tasks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Level is derivable from ParentWorkTaskId, so a rollback restores it exactly.
            migrationBuilder.Sql(
                """
                UPDATE "Tasks"
                SET "Level" = CASE WHEN "ParentWorkTaskId" IS NULL THEN 0 ELSE 1 END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ParentWorkTaskId",
                table: "Tasks",
                column: "ParentWorkTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkProjectId",
                table: "Tasks",
                column: "WorkProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkTicketId",
                table: "Tasks",
                column: "WorkTicketId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tasks_Level",
                table: "Tasks",
                sql: "\"Level\" <= 1");
        }
    }
}
