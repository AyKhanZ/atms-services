using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations;

public partial class SeedPermissionsDataToTable : Migration
{
    private readonly List<(int Id, string Name, string Code, string Module)> items =
    [
        (1, "Role view", "RoleView", "Role"),
        (2, "Role edit", "RoleEdit", "Role"),
        (3, "Role delete", "RoleDelete", "Role"),
        (4, "User view", "UserView", "User"),
        (5, "User edit", "UserEdit", "User"),
        (6, "User delete", "UserDelete", "User"),
        (7, "Project view", "ProjectView", "Project"),
        (8, "Project edit", "ProjectEdit", "Project"),
        (9, "Project delete", "ProjectDelete", "Project"),
        (10, "Comment view", "CommentView", "Comment"),
        (11, "Comment edit", "CommentEdit", "Comment"),
        (12, "Comment delete", "CommentDelete", "Comment"),
        (13, "Notification view", "NotificationView", "Notification"),
        (14, "Notification edit", "NotificationEdit", "Notification"),
        (15, "Notification delete", "NotificationDelete", "Notification")
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var item in items)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: ["Id", "Name", "Code", "Module"],
                values: [item.Id, item.Name, item.Code, item.Module]);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var id in items.Select(x => x.Id))
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: id);
        }
    }
}
