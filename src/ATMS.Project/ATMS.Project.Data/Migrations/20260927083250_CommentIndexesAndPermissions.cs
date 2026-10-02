using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class CommentIndexesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_OwnerType_OwnerId_CreatedAt",
                table: "Comments");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 11, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 11, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_OwnerType_OwnerId_CreatedAt_Id",
                table: "Comments",
                columns: new[] { "OwnerType", "OwnerId", "CreatedAt", "Id" },
                descending: new[] { false, false, true, true },
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_OwnerType_OwnerId_CreatedAt_Id",
                table: "Comments");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 12, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 12, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_OwnerType_OwnerId_CreatedAt",
                table: "Comments",
                columns: new[] { "OwnerType", "OwnerId", "CreatedAt" });
        }
    }
}
