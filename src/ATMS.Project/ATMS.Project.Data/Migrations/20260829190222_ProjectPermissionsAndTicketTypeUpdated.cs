using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectPermissionsAndTicketTypeUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var projectManagerRoleId = new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890");
            var projectMemberRoleIds = new[]
            {
                projectManagerRoleId,
                new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca"),
                new Guid("51805e71-420c-40c4-a074-76b4f29eee7a"),
                new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346"),
                new Guid("6b738142-0c09-47d0-848b-f2d5e411b266")
            };

            foreach (var permissionId in new[] { 4, 7, 10, 13 })
            {
                foreach (var roleId in projectMemberRoleIds)
                {
                    migrationBuilder.DeleteData(
                        table: "RolePermissions",
                        keyColumns: new[] { "PermissionId", "RoleId" },
                        keyValues: new object[] { permissionId, roleId });
                }
            }

            foreach (var permissionId in new[] { 17, 18 })
            {
                migrationBuilder.DeleteData(
                    table: "RolePermissions",
                    keyColumns: new[] { "PermissionId", "RoleId" },
                    keyValues: new object[] { permissionId, projectManagerRoleId });
            }

            foreach (var translationId in new[] { 7, 8, 9, 10, 11, 12, 19, 20, 21, 28, 29, 30, 37, 38, 39, 49, 50, 51, 52, 53, 54 })
            {
                migrationBuilder.DeleteData(
                    table: "PermissionTranslation",
                    keyColumn: "Id",
                    keyValue: translationId);
            }

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 18, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 18, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") });

            foreach (var permissionId in new[] { 3, 4, 7, 10, 13, 17, 18 })
            {
                migrationBuilder.DeleteData(
                    table: "Permissions",
                    keyColumn: "Id",
                    keyValue: permissionId);
            }

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Просмотр проекта");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Редактирование проекта");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Удаление проекта");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Tiketlərə baxış");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 21,
                column: "Name",
                value: "Tapşırıqlara baxış");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 30,
                column: "Name",
                value: "Şərhlərə baxış");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 39,
                column: "Name",
                value: "Bildirişlərə baxış");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 49,
                column: "Name",
                value: "Group and milestone edit");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 50,
                column: "Name",
                value: "Редактирование групп и этапов");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 51,
                column: "Name",
                value: "Qrup və mərhələni redaktə et");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 52,
                column: "Name",
                value: "Group and milestone delete");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 53,
                column: "Name",
                value: "Удаление групп и этапов");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 54,
                column: "Name",
                value: "Qrup və mərhələni sil");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 76,
                column: "Name",
                value: "Participant edit");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 77,
                column: "Name",
                value: "Изменение роли участника");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 78,
                column: "Name",
                value: "İştirakçını redaktə et");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 79,
                column: "Name",
                value: "Participant delete");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 80,
                column: "Name",
                value: "Удаление участника");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 81,
                column: "Name",
                value: "İştirakçını sil");

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Language", "Name", "PermissionId" },
                values: new object[,]
                {
                    { 82, "en", "Invite client participant", 28 },
                    { 83, "ru", "Приглашение участника клиента", 28 },
                    { 84, "az", "Müştəri iştirakçısını dəvət et", 28 }
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "Code",
                value: "ParticipantEdit");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "Code",
                value: "ParticipantDelete");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "Code",
                value: "ParticipantInviteClient");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 29, "TicketCreate" },
                    { 30, "TaskCreate" },
                    { 33, "ParticipantInviteEmployee" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 26, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 27, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 28, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") }
                });

            migrationBuilder.UpdateData(
                table: "WorkTicketTypeTranslation",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Project");

            migrationBuilder.UpdateData(
                table: "WorkTicketTypeTranslation",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Проект");

            migrationBuilder.UpdateData(
                table: "WorkTicketTypeTranslation",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Layihə");

            migrationBuilder.UpdateData(
                table: "WorkTicketTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "Project");

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Language", "Name", "PermissionId" },
                values: new object[,]
                {
                    { 85, "en", "Ticket create", 29 },
                    { 86, "ru", "Создание тикетов", 29 },
                    { 87, "az", "Tiket yarat", 29 },
                    { 88, "en", "Task create", 30 },
                    { 89, "ru", "Создание задач", 30 },
                    { 90, "az", "Tapşırıq yarat", 30 },
                    { 97, "en", "Invite employee participant", 33 },
                    { 98, "ru", "Приглашение сотрудника", 33 },
                    { 99, "az", "Əməkdaş iştirakçını dəvət et", 33 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 29, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 29, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 29, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 29, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 30, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 30, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 30, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 33, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") }
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 26, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 27, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 28, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 29, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 29, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 29, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 29, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 30, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 30, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 30, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 33, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 3, "ProjectDelete" },
                    { 4, "TicketView" },
                    { 7, "TaskView" },
                    { 10, "CommentView" },
                    { 13, "NotificationView" },
                    { 17, "GroupEdit" },
                    { 18, "GroupDelete" }
                });

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Language", "Name", "PermissionId" },
                values: new object[,]
                {
                    { 7, "en", "Project delete", 3 },
                    { 8, "ru", "Удаление проекта", 3 },
                    { 9, "az", "Layihəni sil", 3 },
                    { 10, "en", "Ticket view", 4 },
                    { 11, "ru", "Просмотр тикетов", 4 },
                    { 12, "az", "Tiketlərə baxış", 4 },
                    { 19, "en", "Task view", 7 },
                    { 20, "ru", "Просмотр задач", 7 },
                    { 21, "az", "Tapşırıqlara baxış", 7 },
                    { 28, "en", "Comment view", 10 },
                    { 29, "ru", "Просмотр комментариев", 10 },
                    { 30, "az", "Şərhlərə baxış", 10 },
                    { 37, "en", "Notification view", 13 },
                    { 38, "ru", "Просмотр уведомлений", 13 },
                    { 39, "az", "Bildirişlərə baxış", 13 },
                    { 49, "en", "Group and milestone edit", 17 },
                    { 50, "ru", "Редактирование групп и этапов", 17 },
                    { 51, "az", "Qrup və mərhələni redaktə et", 17 },
                    { 52, "en", "Group and milestone delete", 18 },
                    { 53, "ru", "Удаление групп и этапов", 18 },
                    { 54, "az", "Qrup və mərhələni sil", 18 }
                });

            var projectManagerRoleId = new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890");
            var projectMemberRoleIds = new[]
            {
                projectManagerRoleId,
                new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca"),
                new Guid("51805e71-420c-40c4-a074-76b4f29eee7a"),
                new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346"),
                new Guid("6b738142-0c09-47d0-848b-f2d5e411b266")
            };

            foreach (var permissionId in new[] { 4, 7, 10, 13 })
            {
                foreach (var roleId in projectMemberRoleIds)
                {
                    migrationBuilder.InsertData(
                        table: "RolePermissions",
                        columns: new[] { "PermissionId", "RoleId" },
                        values: new object[] { permissionId, roleId });
                }
            }

            foreach (var permissionId in new[] { 17, 18 })
            {
                migrationBuilder.InsertData(
                    table: "RolePermissions",
                    columns: new[] { "PermissionId", "RoleId" },
                    values: new object[] { permissionId, projectManagerRoleId });
            }

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Просмотр проектов");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Редактирование проектов");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Удаление проектов");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Tiketi baxış");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 21,
                column: "Name",
                value: "Tapşırığa baxış");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 30,
                column: "Name",
                value: "Şərhə baxış");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 39,
                column: "Name",
                value: "Bildirişə baxış");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 49,
                column: "Name",
                value: "Group edit");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 50,
                column: "Name",
                value: "Редактирование групп");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 51,
                column: "Name",
                value: "Qrupu redaktə et");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 52,
                column: "Name",
                value: "Group delete");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 53,
                column: "Name",
                value: "Удаление групп");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 54,
                column: "Name",
                value: "Qrupu sil");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 76,
                column: "Name",
                value: "User edit");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 77,
                column: "Name",
                value: "Редактирование польз.");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 78,
                column: "Name",
                value: "İstifadəçini redaktə");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 79,
                column: "Name",
                value: "User delete");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 80,
                column: "Name",
                value: "Удаление пользователей");

            migrationBuilder.UpdateData(
                table: "PermissionTranslation",
                keyColumn: "Id",
                keyValue: 81,
                column: "Name",
                value: "İstifadəçини sil");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "Code",
                value: "UserEdit");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "Code",
                value: "UserDelete");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "Code",
                value: "UserInvite");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 16, "GroupView" },
                    { 19, "DictionaryView" },
                    { 20, "DictionaryEdit" },
                    { 21, "DictionaryDelete" },
                    { 22, "OrganizationView" },
                    { 23, "OrganizationEdit" },
                    { 24, "OrganizationDelete" },
                    { 25, "UserView" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 2, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 2, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 3, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 3, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 3, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 17, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 17, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 18, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 18, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") }
                });

            migrationBuilder.UpdateData(
                table: "WorkTicketTypeTranslation",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Task");

            migrationBuilder.UpdateData(
                table: "WorkTicketTypeTranslation",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Задача");

            migrationBuilder.UpdateData(
                table: "WorkTicketTypeTranslation",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Tapşırıq");

            migrationBuilder.UpdateData(
                table: "WorkTicketTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "Task");

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Language", "Name", "PermissionId" },
                values: new object[,]
                {
                    { 46, "en", "Group view", 16 },
                    { 47, "ru", "Просмотр групп", 16 },
                    { 48, "az", "Qrupa baxış", 16 },
                    { 55, "en", "Dictionary view", 19 },
                    { 56, "ru", "Просмотр справочников", 19 },
                    { 57, "az", "Lüğətə baxış", 19 },
                    { 58, "en", "Dictionary edit", 20 },
                    { 59, "ru", "Редактирование справочников", 20 },
                    { 60, "az", "Lüğəti redaktə et", 20 },
                    { 61, "en", "Dictionary delete", 21 },
                    { 62, "ru", "Удаление справочников", 21 },
                    { 63, "az", "Lüğəti sil", 21 },
                    { 64, "en", "Organization view", 22 },
                    { 65, "ru", "Просмотр организаций", 22 },
                    { 66, "az", "Təşkilata baxış", 22 },
                    { 67, "en", "Organization edit", 23 },
                    { 68, "ru", "Редактирование организаций", 23 },
                    { 69, "az", "Təşkilatı redaktə et", 23 },
                    { 70, "en", "Organization delete", 24 },
                    { 71, "ru", "Удаление организаций", 24 },
                    { 72, "az", "Təşkilatı sil", 24 },
                    { 73, "en", "User view", 25 },
                    { 74, "ru", "Просмотр пользователей", 25 },
                    { 75, "az", "İstifadəçiyə baxış", 25 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 16, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 16, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 16, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 16, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 16, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") }
                });

        }
    }
}
