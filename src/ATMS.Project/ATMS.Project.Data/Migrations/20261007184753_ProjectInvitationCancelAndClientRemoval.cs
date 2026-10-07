using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectInvitationCancelAndClientRemoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectInvitations_Users_InvitedById",
                table: "ProjectInvitations");

            migrationBuilder.DropIndex(
                name: "IX_ProjectInvitations_InvitedById",
                table: "ProjectInvitations");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code" },
                values: new object[] { 34, "ParticipantDeleteClient" });

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Language", "Name", "PermissionId" },
                values: new object[,]
                {
                    { 100, "en", "Remove client participant", 34 },
                    { 101, "ru", "Удаление участника клиента", 34 },
                    { 102, "az", "Müştəri iştirakçısını sil", 34 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 34, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 34, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_InvitedById",
                table: "ProjectInvitations",
                column: "InvitedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectInvitations_Users_InvitedById",
                table: "ProjectInvitations",
                column: "InvitedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
