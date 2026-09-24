using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkItemHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryEntries_Projects_WorkProjectId",
                        column: x => x.WorkProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoryEntries_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoryChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HistoryEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Field = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryChanges_HistoryEntries_HistoryEntryId",
                        column: x => x.HistoryEntryId,
                        principalTable: "HistoryEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryChanges_HistoryEntryId",
                table: "HistoryChanges",
                column: "HistoryEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEntries_CreatedById",
                table: "HistoryEntries",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEntries_EntityType_EntityId_CreatedAt_Id",
                table: "HistoryEntries",
                columns: new[] { "EntityType", "EntityId", "CreatedAt", "Id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEntries_WorkProjectId_CreatedAt_Id",
                table: "HistoryEntries",
                columns: new[] { "WorkProjectId", "CreatedAt", "Id" },
                descending: new[] { false, true, true });

            // Everything created before the history existed gets its Created entry, without fields:
            // nobody recorded the values it started with. Otherwise old items would have no history
            // at all and their creator would be shown nowhere.
            migrationBuilder.Sql("""
                INSERT INTO "HistoryEntries" ("Id", "WorkProjectId", "EntityType", "EntityId", "Action", "CreatedAt", "CreatedById")
                SELECT gen_random_uuid(), "Id", 1, "Id", 1, "CreatedAt", "CreatedById"
                FROM "Projects" WHERE NOT "IsDeleted";

                INSERT INTO "HistoryEntries" ("Id", "WorkProjectId", "EntityType", "EntityId", "Action", "CreatedAt", "CreatedById")
                SELECT gen_random_uuid(), "WorkProjectId", CASE WHEN "ParentWorkGroupId" IS NULL THEN 2 ELSE 3 END, "Id", 1, "CreatedAt", "CreatedById"
                FROM "ProjectGroups" WHERE NOT "IsDeleted";

                INSERT INTO "HistoryEntries" ("Id", "WorkProjectId", "EntityType", "EntityId", "Action", "CreatedAt", "CreatedById")
                SELECT gen_random_uuid(), "WorkProjectId", 4, "Id", 1, "CreatedAt", "CreatedById"
                FROM "Tickets" WHERE NOT "IsDeleted";

                INSERT INTO "HistoryEntries" ("Id", "WorkProjectId", "EntityType", "EntityId", "Action", "CreatedAt", "CreatedById")
                SELECT gen_random_uuid(), "WorkProjectId", 5, "Id", 1, "CreatedAt", "CreatedById"
                FROM "Tasks" WHERE NOT "IsDeleted";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryChanges");

            migrationBuilder.DropTable(
                name: "HistoryEntries");
        }
    }
}
