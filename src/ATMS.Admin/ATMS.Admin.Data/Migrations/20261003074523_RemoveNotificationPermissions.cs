using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotificationPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Module" },
                values: new object[,]
                {
                    { 10, "NotificationView", "Notification" },
                    { 11, "NotificationEdit", "Notification" },
                    { 12, "NotificationDelete", "Notification" }
                });

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Language", "Name", "PermissionId" },
                values: new object[,]
                {
                    { 37, "en", "Notification view", 10 },
                    { 38, "ru", "Просмотр уведомлений", 10 },
                    { 39, "az", "Bildirişə baxış", 10 },
                    { 40, "en", "Notification edit", 11 },
                    { 41, "ru", "Редактирование уведомлений", 11 },
                    { 42, "az", "Bildirişi redaktə", 11 },
                    { 43, "en", "Notification delete", 12 },
                    { 44, "ru", "Удаление уведомлений", 12 },
                    { 45, "az", "Bildirişi sil", 12 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 10, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 10, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 10, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 10, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 11, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 12, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") }
                });
        }
    }
}
