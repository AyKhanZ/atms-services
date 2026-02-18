using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class InsertDataToPermissionsTable : Migration
    {
        /// <inheritdoc />
        private readonly (int Id, string Name, string Code, string Module)[] _permissions =
        [
            (1, "Role View", "RoleView", "Role"),
            (2, "Role Edit", "RoleEdit", "Role"),
            (3, "Role Create", "RoleCreate", "Role"),

            (4, "User View", "UserView", "User"),
            (5, "User Edit", "UserEdit", "User"),
            (6, "User Create", "UserCreate", "User"),

            (7, "Project View", "ProjectView", "Project"),
            (8, "Project Edit", "ProjectEdit", "Project"),
            (9, "Project Create", "ProjectCreate", "Project"),

            (10, "Comment View", "CommentView", "Comment"),
            (11, "Comment Edit", "CommentEdit", "Comment"),
            (12, "Comment Create", "CommentCreate", "Comment"),

            (13, "Notification View", "NotificationView", "Notification"),
            (14, "Notification Edit", "NotificationEdit", "Notification"),
            (15, "Notification Create", "NotificationCreate", "Notification"),
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var permission in _permissions)
            {
                migrationBuilder.InsertData(
                    table: "Permissions",
                    columns: new[] { "Id", "Name", "Code", "Module" },
                    values: new object[] { permission.Id, permission.Name, permission.Code, permission.Module }
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
}