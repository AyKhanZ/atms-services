using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
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
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false)
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
                name: "PersonalInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    AvatarPath = table.Column<string>(type: "text", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserProgressId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenderId = table.Column<int>(type: "integer", nullable: false),
                    MaritalStatusId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalInfo_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalInfo_MaritalStatuses_MaritalStatusId",
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
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    InvitedById = table.Column<Guid>(type: "uuid", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserStatusId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    MaritalStatusId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    GenderId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
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
                        name: "FK_Users_Users_InvitedById",
                        column: x => x.InvitedById,
                        principalTable: "Users",
                        principalColumn: "Id");
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
                name: "UserProgresses",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProgressType = table.Column<int>(type: "integer", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PersonalInfoId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgresses", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProgresses_PersonalInfo_PersonalInfoId",
                        column: x => x.PersonalInfoId,
                        principalTable: "PersonalInfo",
                        principalColumn: "Id");
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

            migrationBuilder.CreateTable(
                name: "InvitedUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProgressId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitedUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitedUser_UserProgresses_UserProgressId",
                        column: x => x.UserProgressId,
                        principalTable: "UserProgresses",
                        principalColumn: "UserId",
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
                columns: new[] { "Id", "Description", "IsSystem", "Name", "UserType" },
                values: new object[,]
                {
                    { new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5"), "Client Manager Role", true, "Client Manager", 3 },
                    { new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53"), "BAIM employee role", true, "Employee", 2 }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "IsAdmin", "IsSystem", "Name", "UserType" },
                values: new object[] { new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f"), "Technical administrator with all system permissions", true, true, "SuperAdmin", 1 });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "IsSystem", "Name", "UserType" },
                values: new object[] { new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8"), "Client Role", true, "Client", 3 });

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
                    { 28, "en", "Comment view", 13 },
                    { 29, "ru", "Просмотр комментариев", 13 },
                    { 30, "az", "Şərhə baxış", 13 },
                    { 31, "en", "Comment edit", 14 },
                    { 32, "ru", "Редактирование комментариев", 14 },
                    { 33, "az", "Şərhi redaktə", 14 },
                    { 34, "en", "Comment delete", 15 },
                    { 35, "ru", "Удаление комментариев", 15 },
                    { 36, "az", "Şərhi sil", 15 },
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
                    { 1, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 1, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 1, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 1, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 2, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 3, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 4, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 4, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 4, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 4, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 5, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 6, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 7, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 7, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 7, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 7, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 8, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 8, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 9, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 10, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 10, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 10, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 10, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 11, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 12, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 13, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 13, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 13, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 13, new Guid("dc91d07f-2a00-486b-8a90-aa7b4c688de8") },
                    { 14, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 14, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 14, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") },
                    { 15, new Guid("4c0a7e27-0576-4738-9f73-1d9cc14374a5") },
                    { 15, new Guid("58a8f620-1550-41a2-8693-336fd9bbeb53") },
                    { 15, new Guid("cc4b9105-86b8-49ca-9b2f-260551aa675f") }
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
                name: "IX_InvitedUser_Email",
                table: "InvitedUser",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvitedUser_UserProgressId",
                table: "InvitedUser",
                column: "UserProgressId");

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
                name: "IX_PersonalInfo_GenderId",
                table: "PersonalInfo",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalInfo_MaritalStatusId",
                table: "PersonalInfo",
                column: "MaritalStatusId");

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
                name: "IX_Roles_UserType",
                table: "Roles",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_PersonalInfoId",
                table: "UserProgresses",
                column: "PersonalInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

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
                name: "IX_UserStatuses_Code",
                table: "UserStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStatusTranslation_UserStatusId_Language",
                table: "UserStatusTranslation",
                columns: new[] { "UserStatusId", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenderTranslation");

            migrationBuilder.DropTable(
                name: "InvitedUser");

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
                name: "UserProgresses");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PersonalInfo");

            migrationBuilder.DropTable(
                name: "UserStatuses");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "MaritalStatuses");
        }
    }
}
