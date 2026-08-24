using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserAuditRelationsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "IsAdmin" = TRUE
                WHERE "UserType" = 1;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO "Users" ("Id", "Email", "Name", "Surname", "UserType", "AvatarPath", "IsDeleted", "IsAdmin")
                SELECT
                    '00000000-0000-0000-0000-000000000001'::uuid,
                    'system-administrator@atms.local',
                    'System',
                    'Administrator',
                    1,
                    'default-avatar.png',
                    FALSE,
                    TRUE
                WHERE NOT EXISTS (SELECT 1 FROM "Users")
                  AND (
                    EXISTS (SELECT 1 FROM "Organizations")
                    OR EXISTS (SELECT 1 FROM "Projects")
                    OR EXISTS (SELECT 1 FROM "ProjectGroups")
                    OR EXISTS (SELECT 1 FROM "Tickets")
                    OR EXISTS (SELECT 1 FROM "Tasks")
                    OR EXISTS (SELECT 1 FROM "Meetings")
                    OR EXISTS (SELECT 1 FROM "MeetingMinutes")
                    OR EXISTS (SELECT 1 FROM "Comments")
                    OR EXISTS (SELECT 1 FROM "Attachments")
                  );

                UPDATE "Organizations"
                SET "CreatedById" = COALESCE(
                    (SELECT "Id" FROM "Users" WHERE "IsAdmin" = TRUE ORDER BY "Email" LIMIT 1),
                    (SELECT "Id" FROM "Users" ORDER BY "Email" LIMIT 1)
                )
                WHERE "CreatedById" IS NULL;

                WITH audit_user_ids AS (
                    SELECT "CreatedById" AS "Id" FROM "Organizations"
                    UNION SELECT "UpdatedById" FROM "Organizations" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "DeletedById" FROM "Organizations" WHERE "DeletedById" IS NOT NULL
                    UNION SELECT "CreatedById" FROM "Projects"
                    UNION SELECT "UpdatedById" FROM "Projects" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "DeletedById" FROM "Projects" WHERE "DeletedById" IS NOT NULL
                    UNION SELECT "CreatedById" FROM "ProjectGroups"
                    UNION SELECT "UpdatedById" FROM "ProjectGroups" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "DeletedById" FROM "ProjectGroups" WHERE "DeletedById" IS NOT NULL
                    UNION SELECT "CreatedById" FROM "Tickets"
                    UNION SELECT "UpdatedById" FROM "Tickets" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "DeletedById" FROM "Tickets" WHERE "DeletedById" IS NOT NULL
                    UNION SELECT "CreatedById" FROM "Tasks"
                    UNION SELECT "UpdatedById" FROM "Tasks" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "DeletedById" FROM "Tasks" WHERE "DeletedById" IS NOT NULL
                    UNION SELECT "CreatedById" FROM "Meetings"
                    UNION SELECT "UpdatedById" FROM "Meetings" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "DeletedById" FROM "Meetings" WHERE "DeletedById" IS NOT NULL
                    UNION SELECT "CreatedById" FROM "MeetingMinutes"
                    UNION SELECT "UpdatedById" FROM "MeetingMinutes" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "CreatedById" FROM "Comments"
                    UNION SELECT "UpdatedById" FROM "Comments" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "DeletedById" FROM "Comments" WHERE "DeletedById" IS NOT NULL
                    UNION SELECT "CreatedById" FROM "Attachments"
                    UNION SELECT "UpdatedById" FROM "Attachments" WHERE "UpdatedById" IS NOT NULL
                    UNION SELECT "DeletedById" FROM "Attachments" WHERE "DeletedById" IS NOT NULL
                )
                INSERT INTO "Users" ("Id", "Email", "Name", "Surname", "UserType", "AvatarPath", "IsDeleted", "IsAdmin")
                SELECT
                    ids."Id",
                    'audit-user-' || ids."Id" || '@atms.local',
                    'System',
                    'Administrator',
                    1,
                    'default-avatar.png',
                    FALSE,
                    TRUE
                FROM audit_user_ids ids
                WHERE ids."Id" IS NOT NULL
                  AND ids."Id" <> '00000000-0000-0000-0000-000000000000'::uuid
                  AND NOT EXISTS (
                    SELECT 1
                    FROM "Users" users
                    WHERE users."Id" = ids."Id"
                  );
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "Organizations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsAdmin",
                table: "Users",
                column: "IsAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedById",
                table: "Tickets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DeletedById",
                table: "Tickets",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UpdatedById",
                table: "Tickets",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedById",
                table: "Tasks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DeletedById",
                table: "Tasks",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UpdatedById",
                table: "Tasks",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DeletedById",
                table: "Projects",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_CreatedById",
                table: "ProjectGroups",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_DeletedById",
                table: "ProjectGroups",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_UpdatedById",
                table: "ProjectGroups",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CreatedById",
                table: "Organizations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_DeletedById",
                table: "Organizations",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_UpdatedById",
                table: "Organizations",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CreatedById",
                table: "Meetings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_DeletedById",
                table: "Meetings",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_UpdatedById",
                table: "Meetings",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingMinutes_CreatedById",
                table: "MeetingMinutes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingMinutes_UpdatedById",
                table: "MeetingMinutes",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedById",
                table: "Comments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DeletedById",
                table: "Comments",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UpdatedById",
                table: "Comments",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_CreatedById",
                table: "Attachments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_DeletedById",
                table: "Attachments",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UpdatedById",
                table: "Attachments",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Users_CreatedById",
                table: "Attachments",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Users_DeletedById",
                table: "Attachments",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Users_UpdatedById",
                table: "Attachments",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_CreatedById",
                table: "Comments",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_DeletedById",
                table: "Comments",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_UpdatedById",
                table: "Comments",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingMinutes_Users_CreatedById",
                table: "MeetingMinutes",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingMinutes_Users_UpdatedById",
                table: "MeetingMinutes",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_CreatedById",
                table: "Meetings",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_DeletedById",
                table: "Meetings",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_UpdatedById",
                table: "Meetings",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Users_CreatedById",
                table: "Organizations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Users_DeletedById",
                table: "Organizations",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Users_UpdatedById",
                table: "Organizations",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectGroups_Users_CreatedById",
                table: "ProjectGroups",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectGroups_Users_DeletedById",
                table: "ProjectGroups",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectGroups_Users_UpdatedById",
                table: "ProjectGroups",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_DeletedById",
                table: "Projects",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_CreatedById",
                table: "Tasks",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_DeletedById",
                table: "Tasks",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_UpdatedById",
                table: "Tasks",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_CreatedById",
                table: "Tickets",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_DeletedById",
                table: "Tickets",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_UpdatedById",
                table: "Tickets",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Users_CreatedById",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Users_DeletedById",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Users_UpdatedById",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_CreatedById",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_DeletedById",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_UpdatedById",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingMinutes_Users_CreatedById",
                table: "MeetingMinutes");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingMinutes_Users_UpdatedById",
                table: "MeetingMinutes");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_CreatedById",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_DeletedById",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_UpdatedById",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Users_CreatedById",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Users_DeletedById",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Users_UpdatedById",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectGroups_Users_CreatedById",
                table: "ProjectGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectGroups_Users_DeletedById",
                table: "ProjectGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectGroups_Users_UpdatedById",
                table: "ProjectGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_DeletedById",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_CreatedById",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_DeletedById",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_UpdatedById",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_CreatedById",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_DeletedById",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_UpdatedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsAdmin",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CreatedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DeletedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UpdatedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedById",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_DeletedById",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UpdatedById",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_DeletedById",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectGroups_CreatedById",
                table: "ProjectGroups");

            migrationBuilder.DropIndex(
                name: "IX_ProjectGroups_DeletedById",
                table: "ProjectGroups");

            migrationBuilder.DropIndex(
                name: "IX_ProjectGroups_UpdatedById",
                table: "ProjectGroups");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CreatedById",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_DeletedById",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_UpdatedById",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CreatedById",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_DeletedById",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_UpdatedById",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_MeetingMinutes_CreatedById",
                table: "MeetingMinutes");

            migrationBuilder.DropIndex(
                name: "IX_MeetingMinutes_UpdatedById",
                table: "MeetingMinutes");

            migrationBuilder.DropIndex(
                name: "IX_Comments_CreatedById",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_DeletedById",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UpdatedById",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_CreatedById",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_DeletedById",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_UpdatedById",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Organizations");
        }
    }
}
