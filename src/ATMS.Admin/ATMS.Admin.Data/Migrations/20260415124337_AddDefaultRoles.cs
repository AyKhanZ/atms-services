using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaritalStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaritalStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenderTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GenderId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenderTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenderTranslation_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaritalStatusTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaritalStatusId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaritalStatusTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaritalStatusTranslation_MaritalStatuses_MaritalStatusId",
                        column: x => x.MaritalStatusId,
                        principalTable: "MaritalStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermissionId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionTranslation_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.PermissionId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStatusTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatusTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStatusTranslation_UserStatuses_UserStatusId",
                        column: x => x.UserStatusId,
                        principalTable: "UserStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvatarPath = table.Column<string>(type: "text", nullable: false, defaultValue: "default-avatar.png"),
                    Position = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    HasCompletedSurvey = table.Column<bool>(type: "boolean", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    FailedLoginCount = table.Column<long>(type: "bigint", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: false, defaultValue: "en"),
                    InvitedById = table.Column<Guid>(type: "uuid", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserStatusId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    MaritalStatusId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    GenderId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UserTypeId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Surname = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_MaritalStatuses_MaritalStatusId",
                        column: x => x.MaritalStatusId,
                        principalTable: "MaritalStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_UserStatuses_UserStatusId",
                        column: x => x.UserStatusId,
                        principalTable: "UserStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_UserTypes_UserTypeId",
                        column: x => x.UserTypeId,
                        principalTable: "UserTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Users_InvitedById",
                        column: x => x.InvitedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTypeTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserTypeId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypeTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTypeTranslation_UserTypes_UserTypeId",
                        column: x => x.UserTypeId,
                        principalTable: "UserTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshRevokedTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshRevokedTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshRevokedTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Genders",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "NotSpecified" },
                    { 2, "Male" },
                    { 3, "Female" },
                    { 4, "Other" }
                });

            migrationBuilder.InsertData(
                table: "MaritalStatuses",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "NotSpecified" },
                    { 2, "Single" },
                    { 3, "Married" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Module" },
                values: new object[,]
                {
                    { 1, "RoleView", "Role" },
                    { 2, "RoleEdit", "Role" },
                    { 3, "RoleDelete", "Role" },
                    { 4, "UserView", "User" },
                    { 5, "UserEdit", "User" },
                    { 6, "UserDelete", "User" },
                    { 7, "ProjectView", "Project" },
                    { 8, "ProjectEdit", "Project" },
                    { 9, "ProjectDelete", "Project" },
                    { 10, "NotificationView", "Notification" },
                    { 11, "NotificationEdit", "Notification" },
                    { 12, "NotificationDelete", "Notification" },
                    { 13, "CommentView", "Comment" },
                    { 14, "CommentEdit", "Comment" },
                    { 15, "CommentDelete", "Comment" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5"), "Client Manager Role", "Client Manager" },
                    { new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53"), "Agent Role", "Agent" },
                    { new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8"), "Client Role", "Client" }
                });

            migrationBuilder.InsertData(
                table: "UserStatuses",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Inactive" },
                    { 3, "Locked" }
                });

            migrationBuilder.InsertData(
                table: "UserTypes",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "Agent" },
                    { 2, "Client" }
                });

            migrationBuilder.InsertData(
                table: "GenderTranslation",
                columns: new[] { "Id", "GenderId", "Language", "Name" },
                values: new object[,]
                {
                    { 1, 1, "en", "Not specified" },
                    { 2, 1, "ru", "Не указано" },
                    { 3, 1, "az", "Göstərilməyib" },
                    { 4, 2, "en", "Male" },
                    { 5, 2, "ru", "Мужской" },
                    { 6, 2, "az", "Kişi" },
                    { 7, 3, "en", "Female" },
                    { 8, 3, "ru", "Женский" },
                    { 9, 3, "az", "Qadın" },
                    { 10, 4, "en", "Other" },
                    { 11, 4, "ru", "Другое" },
                    { 12, 4, "az", "Digər" }
                });

            migrationBuilder.InsertData(
                table: "MaritalStatusTranslation",
                columns: new[] { "Id", "Language", "MaritalStatusId", "Name" },
                values: new object[,]
                {
                    { 1, "en", 1, "Not specified" },
                    { 2, "ru", 1, "Не указано" },
                    { 3, "az", 1, "Göstərilməyib" },
                    { 4, "en", 2, "Single" },
                    { 5, "ru", 2, "Холост" },
                    { 6, "az", 2, "Subay" },
                    { 7, "en", 3, "Married" },
                    { 8, "ru", 3, "Женат" },
                    { 9, "az", 3, "Evli" }
                });

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Language", "Name", "PermissionId" },
                values: new object[,]
                {
                    { 1, "en", "Role View", 1 },
                    { 2, "ru", "Просмотр ролей", 1 },
                    { 3, "az", "Rola baxış", 1 },
                    { 4, "en", "Role edit", 2 },
                    { 5, "ru", "Редактирование ролей", 2 },
                    { 6, "az", "Rolu redaktə et", 2 },
                    { 7, "en", "Role delete", 3 },
                    { 8, "ru", "Удаление ролей", 3 },
                    { 9, "az", "Rolu sil", 3 },
                    { 10, "en", "User view", 4 },
                    { 11, "ru", "Просмотр пользователей", 4 },
                    { 12, "az", "İstifadəçiyə baxış", 4 },
                    { 13, "en", "User edit", 5 },
                    { 14, "ru", "Редактирование пользователей", 5 },
                    { 15, "az", "İstifadəçini redaktə", 5 },
                    { 16, "en", "User delete", 6 },
                    { 17, "ru", "Удаление пользователей", 6 },
                    { 18, "az", "İstifadəçini sil", 6 },
                    { 19, "en", "Project view", 7 },
                    { 20, "ru", "Просмотр проектов", 7 },
                    { 21, "az", "Layihəyə baxış", 7 },
                    { 22, "en", "Project edit", 8 },
                    { 23, "ru", "Редактирование проектов", 8 },
                    { 24, "az", "Layihəni redaktə", 8 },
                    { 25, "en", "Project delete", 9 },
                    { 26, "ru", "Удаление проектов", 9 },
                    { 27, "az", "Layihəni sil", 9 },
                    { 28, "en", "Comment view", 10 },
                    { 29, "ru", "Просмотр комментариев", 10 },
                    { 30, "az", "Şərhə baxış", 10 },
                    { 31, "en", "Comment edit", 11 },
                    { 32, "ru", "Редактирование комментариев", 11 },
                    { 33, "az", "Şərhi redaktə", 11 },
                    { 34, "en", "Comment delete", 12 },
                    { 35, "ru", "Удаление комментариев", 12 },
                    { 36, "az", "Şərhi sil", 12 },
                    { 37, "en", "Notification view", 13 },
                    { 38, "ru", "Просмотр уведомлений", 13 },
                    { 39, "az", "Bildirişə baxış", 13 },
                    { 40, "en", "Notification edit", 14 },
                    { 41, "ru", "Редактирование уведомлений", 14 },
                    { 42, "az", "Bildirişi redaktə", 14 },
                    { 43, "en", "Notification delete", 15 },
                    { 44, "ru", "Удаление уведомлений", 15 },
                    { 45, "az", "Bildirişi sil", 15 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 1, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 1, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 4, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 4, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 4, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 7, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 7, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 7, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 8, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 10, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 10, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 10, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 13, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 13, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 13, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 14, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 14, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 15, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 15, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") }
                });

            migrationBuilder.InsertData(
                table: "UserStatusTranslation",
                columns: new[] { "Id", "Language", "Name", "UserStatusId" },
                values: new object[,]
                {
                    { 1, "en", "Active", 1 },
                    { 2, "ru", "Активный", 1 },
                    { 3, "az", "Aktiv", 1 },
                    { 4, "en", "Inactive", 2 },
                    { 5, "ru", "Неактивный", 2 },
                    { 6, "az", "Qeyri-aktiv", 2 },
                    { 7, "en", "Locked", 3 },
                    { 8, "ru", "Заблокирован", 3 },
                    { 9, "az", "Bloklanmış", 3 }
                });

            migrationBuilder.InsertData(
                table: "UserTypeTranslation",
                columns: new[] { "Id", "Language", "Name", "UserTypeId" },
                values: new object[,]
                {
                    { 1, "en", "Agent", 1 },
                    { 2, "ru", "Агент", 1 },
                    { 3, "az", "Agent", 1 },
                    { 4, "en", "Client", 2 },
                    { 5, "ru", "Клиент", 2 },
                    { 6, "az", "Müştəri", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Genders_Code",
                table: "Genders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenderTranslation_GenderId_Language",
                table: "GenderTranslation",
                columns: new[] { "GenderId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaritalStatuses_Code",
                table: "MaritalStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaritalStatusTranslation_MaritalStatusId_Language",
                table: "MaritalStatusTranslation",
                columns: new[] { "MaritalStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTranslation_PermissionId_Language",
                table: "PermissionTranslation",
                columns: new[] { "PermissionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshRevokedTokens_Token",
                table: "RefreshRevokedTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshRevokedTokens_UserId",
                table: "RefreshRevokedTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GenderId",
                table: "Users",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_InvitedById",
                table: "Users",
                column: "InvitedById");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MaritalStatusId",
                table: "Users",
                column: "MaritalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RefreshToken",
                table: "Users",
                column: "RefreshToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserStatusId",
                table: "Users",
                column: "UserStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserTypeId",
                table: "Users",
                column: "UserTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatuses_Code",
                table: "UserStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStatusTranslation_UserStatusId_Language",
                table: "UserStatusTranslation",
                columns: new[] { "UserStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTypes_Code",
                table: "UserTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTypeTranslation_UserTypeId_Language",
                table: "UserTypeTranslation",
                columns: new[] { "UserTypeId", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenderTranslation");

            migrationBuilder.DropTable(
                name: "MaritalStatusTranslation");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "PermissionTranslation");

            migrationBuilder.DropTable(
                name: "RefreshRevokedTokens");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserStatusTranslation");

            migrationBuilder.DropTable(
                name: "UserTypeTranslation");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "MaritalStatuses");

            migrationBuilder.DropTable(
                name: "UserStatuses");

            migrationBuilder.DropTable(
                name: "UserTypes");
        }
    }
}
