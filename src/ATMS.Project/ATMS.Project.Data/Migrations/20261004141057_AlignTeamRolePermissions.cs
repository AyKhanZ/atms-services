using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignTeamRolePermissions : Migration
    {
        // Developer and Business Consultant.
        private const string TeamRoles =
            "'51805e71-420c-40c4-a074-76b4f29eee7a', '7b59a306-3455-4d35-bb7d-d7a07e8219ca'";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Initial migration gave every role every permission, and later migrations removed only
            // what the model knew about. Every database built from them still lets the two team roles
            // delete someone else's comment (CommentDelete = 12): only the project manager may.
            migrationBuilder.Sql($"""
                DELETE FROM "RolePermissions"
                WHERE "PermissionId" = 12 AND "RoleId" IN ({TeamRoles});
                """);

            // Ticket and task rights the team roles already have in every such database, now written in
            // the model too. ON CONFLICT: the rows are there, a plain insert would fail on the key.
            migrationBuilder.Sql($"""
                INSERT INTO "RolePermissions" ("PermissionId", "RoleId")
                SELECT permission.id, role.id
                FROM unnest(ARRAY[5, 6, 8, 9, 29, 30]) AS permission(id)
                CROSS JOIN unnest(ARRAY[{TeamRoles}]::uuid[]) AS role(id)
                ON CONFLICT ("PermissionId", "RoleId") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ticket and task rights stay: every database had them before this migration.
            migrationBuilder.Sql($"""
                INSERT INTO "RolePermissions" ("PermissionId", "RoleId")
                SELECT 12, role.id
                FROM unnest(ARRAY[{TeamRoles}]::uuid[]) AS role(id)
                ON CONFLICT ("PermissionId", "RoleId") DO NOTHING;
                """);
        }
    }
}
