using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class CommentClientDeletePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 15, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") });
        }
    }
}
