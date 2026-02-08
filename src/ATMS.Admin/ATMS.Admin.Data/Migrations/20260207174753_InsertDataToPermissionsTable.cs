using Microsoft.EntityFrameworkCore.Migrations;

namespace ATMS.Admin.Data.Migrations;

public partial class InsertDataToPermissionsTable : Migration
{
    private readonly (Guid Id, string Name)[] _permissions =
    [
        (Guid.Parse("11111111-1111-1111-1111-111111111111"), "RoleView"),
        (Guid.Parse("11111111-1111-1111-1111-111111111112"), "RoleEdit"),
        (Guid.Parse("11111111-1111-1111-1111-111111111113"), "RoleCreate"),

        (Guid.Parse("11111111-1111-1111-1111-111111111114"), "UserView"),
        (Guid.Parse("11111111-1111-1111-1111-111111111115"), "UserEdit"),
        (Guid.Parse("11111111-1111-1111-1111-111111111116"), "UserCreate"),

        (Guid.Parse("11111111-1111-1111-1111-111111111117"), "ProjectView"),
        (Guid.Parse("11111111-1111-1111-1111-111111111118"), "ProjectEdit"),
        (Guid.Parse("11111111-1111-1111-1111-111111111119"), "ProjectCreate"),

        (Guid.Parse("11111111-1111-1111-1111-111111111120"), "CommentView"),
        (Guid.Parse("11111111-1111-1111-1111-111111111121"), "CommentEdit"),
        (Guid.Parse("11111111-1111-1111-1111-111111111122"), "CommentCreate"),

        (Guid.Parse("11111111-1111-1111-1111-111111111123"), "NotificationView"),
        (Guid.Parse("11111111-1111-1111-1111-111111111124"), "NotificationEdit"),
        (Guid.Parse("11111111-1111-1111-1111-111111111125"), "NotificationCreate"),
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var permission in _permissions)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[] { permission.Id, permission.Name }
            );
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var permission in _permissions)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: permission.Id
            );
        }
    }
}
