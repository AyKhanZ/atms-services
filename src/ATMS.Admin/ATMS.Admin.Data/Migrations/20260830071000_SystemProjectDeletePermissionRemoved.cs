using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    public partial class SystemProjectDeletePermissionRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "RolePermissions"
                WHERE "PermissionId" = 9
                  AND "RoleId" = 'cc4b9105-86b8-49ca-9b2f-260551aa675f';
                """);

            migrationBuilder.Sql("""
                DELETE FROM "PermissionTranslation"
                WHERE "Id" IN (25, 26, 27);
                """);

            migrationBuilder.Sql("""
                DELETE FROM "Permissions"
                WHERE "Id" = 9;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "Permissions" ("Id", "Code", "Module")
                VALUES (9, 'ProjectDelete', 'Project')
                ON CONFLICT ("Id") DO NOTHING;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "PermissionTranslation" ("Id", "Language", "Name", "PermissionId")
                VALUES
                    (25, 'en', 'Project delete', 9),
                    (26, 'ru', 'Удаление проектов', 9),
                    (27, 'az', 'Layihəni sil', 9)
                ON CONFLICT ("Id") DO NOTHING;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "RolePermissions" ("PermissionId", "RoleId")
                VALUES (9, 'cc4b9105-86b8-49ca-9b2f-260551aa675f')
                ON CONFLICT ("PermissionId", "RoleId") DO NOTHING;
                """);
        }
    }
}
