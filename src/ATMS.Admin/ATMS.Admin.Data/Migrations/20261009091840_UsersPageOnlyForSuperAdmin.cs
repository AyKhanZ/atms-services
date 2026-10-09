using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class UsersPageOnlyForSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 4, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 4, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 4, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") }
                });
        }
    }
}
