using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Voen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LogoPath = table.Column<string>(type: "text", nullable: true, defaultValue: "logo path"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectKinds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectKinds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkGroupStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkGroupStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemPriorities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemPriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTaskStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTaskStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTicketStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTicketStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTicketTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTicketTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserTypeId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Surname = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PermissionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermissionId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionTranslations_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectKindTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectKindId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectKindTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectKindTranslations_ProjectKinds_ProjectKindId",
                        column: x => x.ProjectKindId,
                        principalTable: "ProjectKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatusTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatusTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectStatusTranslations_ProjectStatuses_ProjectStatusId",
                        column: x => x.ProjectStatusId,
                        principalTable: "ProjectStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTypeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ProjectTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTypeTranslations_ProjectTypes_ProjectTypeId",
                        column: x => x.ProjectTypeId,
                        principalTable: "ProjectTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectTypeId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ProjectKindId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ProjectStatusId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkProjects_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkProjects_ProjectKinds_ProjectKindId",
                        column: x => x.ProjectKindId,
                        principalTable: "ProjectKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkProjects_ProjectStatuses_ProjectStatusId",
                        column: x => x.ProjectStatusId,
                        principalTable: "ProjectStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkProjects_ProjectTypes_ProjectTypeId",
                        column: x => x.ProjectTypeId,
                        principalTable: "ProjectTypes",
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
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
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
                name: "WorkGroupStatusTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkGroupStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkGroupStatusTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkGroupStatusTranslations_WorkGroupStatuses_WorkGroupStat~",
                        column: x => x.WorkGroupStatusId,
                        principalTable: "WorkGroupStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemPriorityTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkItemPriorityId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemPriorityTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItemPriorityTranslations_WorkItemPriorities_WorkItemPri~",
                        column: x => x.WorkItemPriorityId,
                        principalTable: "WorkItemPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTaskStatusTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkTaskStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTaskStatusTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTaskStatusTranslations_WorkTaskStatuses_WorkTaskStatusId",
                        column: x => x.WorkTaskStatusId,
                        principalTable: "WorkTaskStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTicketStatusTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkTicketStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTicketStatusTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTicketStatusTranslations_WorkTicketStatuses_WorkTicketS~",
                        column: x => x.WorkTicketStatusId,
                        principalTable: "WorkTicketStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTicketTypeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkTicketTypeId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTicketTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTicketTypeTranslations_WorkTicketTypes_WorkTicketTypeId",
                        column: x => x.WorkTicketTypeId,
                        principalTable: "WorkTicketTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentWorkGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    WorkProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkGroups", x => x.Id);
                    table.CheckConstraint("CK_WorkGroup_Level", "\"Level\" <= 1");
                    table.ForeignKey(
                        name: "FK_WorkGroups_WorkGroupStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "WorkGroupStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkGroups_WorkGroups_ParentWorkGroupId",
                        column: x => x.ParentWorkGroupId,
                        principalTable: "WorkGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkGroups_WorkProjects_WorkProjectId",
                        column: x => x.WorkProjectId,
                        principalTable: "WorkProjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkProjectParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkProjectParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkProjectParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkProjectParticipants_WorkProjects_WorkProjectId",
                        column: x => x.WorkProjectId,
                        principalTable: "WorkProjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkProjectParticipantRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkProjectParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkProjectParticipantRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkProjectParticipantRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkProjectParticipantRoles_WorkProjectParticipants_WorkPro~",
                        column: x => x.WorkProjectParticipantId,
                        principalTable: "WorkProjectParticipants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkTicketTypeId = table.Column<int>(type: "integer", nullable: false),
                    WorkTicketStatusId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    PriorityId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AssigneeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTickets_WorkGroups_WorkGroupId",
                        column: x => x.WorkGroupId,
                        principalTable: "WorkGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkTickets_WorkItemPriorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "WorkItemPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTickets_WorkProjectParticipants_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "WorkProjectParticipants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkTickets_WorkProjects_WorkProjectId",
                        column: x => x.WorkProjectId,
                        principalTable: "WorkProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTickets_WorkTaskStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "WorkTaskStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTickets_WorkTicketStatuses_WorkTicketStatusId",
                        column: x => x.WorkTicketStatusId,
                        principalTable: "WorkTicketStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTickets_WorkTicketTypes_WorkTicketTypeId",
                        column: x => x.WorkTicketTypeId,
                        principalTable: "WorkTicketTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentWorkTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<long>(type: "bigint", nullable: false),
                    WorkTicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PriorityId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AssigneeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTasks", x => x.Id);
                    table.CheckConstraint("CK_WorkTask_Level", "\"Level\" <= 1");
                    table.ForeignKey(
                        name: "FK_WorkTasks_WorkItemPriorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "WorkItemPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTasks_WorkProjectParticipants_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "WorkProjectParticipants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkTasks_WorkProjects_WorkProjectId",
                        column: x => x.WorkProjectId,
                        principalTable: "WorkProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTasks_WorkTaskStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "WorkTaskStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTasks_WorkTasks_ParentWorkTaskId",
                        column: x => x.ParentWorkTaskId,
                        principalTable: "WorkTasks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkTasks_WorkTickets_WorkTicketId",
                        column: x => x.WorkTicketId,
                        principalTable: "WorkTickets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Title",
                table: "Organizations",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Voen",
                table: "Organizations",
                column: "Voen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTranslations_PermissionId_Language",
                table: "PermissionTranslations",
                columns: new[] { "PermissionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectKinds_Code",
                table: "ProjectKinds",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectKindTranslations_ProjectKindId_Language",
                table: "ProjectKindTranslations",
                columns: new[] { "ProjectKindId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatuses_Code",
                table: "ProjectStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatusTranslations_ProjectStatusId_Language",
                table: "ProjectStatusTranslations",
                columns: new[] { "ProjectStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTypes_Code",
                table: "ProjectTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTypeTranslations_ProjectTypeId_Language",
                table: "ProjectTypeTranslations",
                columns: new[] { "ProjectTypeId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkGroups_Code",
                table: "WorkGroups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkGroups_ParentWorkGroupId",
                table: "WorkGroups",
                column: "ParentWorkGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkGroups_StatusId",
                table: "WorkGroups",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkGroups_Title",
                table: "WorkGroups",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkGroups_WorkProjectId_ParentWorkGroupId_Title",
                table: "WorkGroups",
                columns: new[] { "WorkProjectId", "ParentWorkGroupId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkGroupStatuses_Code",
                table: "WorkGroupStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkGroupStatusTranslations_WorkGroupStatusId_Language",
                table: "WorkGroupStatusTranslations",
                columns: new[] { "WorkGroupStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemPriorities_Code",
                table: "WorkItemPriorities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemPriorityTranslations_WorkItemPriorityId_Language",
                table: "WorkItemPriorityTranslations",
                columns: new[] { "WorkItemPriorityId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjectParticipantRoles_RoleId",
                table: "WorkProjectParticipantRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjectParticipantRoles_WorkProjectParticipantId_RoleId",
                table: "WorkProjectParticipantRoles",
                columns: new[] { "WorkProjectParticipantId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjectParticipants_UserId",
                table: "WorkProjectParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjectParticipants_WorkProjectId_UserId",
                table: "WorkProjectParticipants",
                columns: new[] { "WorkProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjects_Code",
                table: "WorkProjects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjects_OrganizationId_Title",
                table: "WorkProjects",
                columns: new[] { "OrganizationId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjects_ProjectKindId",
                table: "WorkProjects",
                column: "ProjectKindId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjects_ProjectStatusId",
                table: "WorkProjects",
                column: "ProjectStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjects_ProjectTypeId",
                table: "WorkProjects",
                column: "ProjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProjects_Title",
                table: "WorkProjects",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_AssigneeId",
                table: "WorkTasks",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_Code",
                table: "WorkTasks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_ParentWorkTaskId",
                table: "WorkTasks",
                column: "ParentWorkTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_PriorityId",
                table: "WorkTasks",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_StatusId",
                table: "WorkTasks",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_WorkProjectId",
                table: "WorkTasks",
                column: "WorkProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_WorkTicketId",
                table: "WorkTasks",
                column: "WorkTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTaskStatuses_Code",
                table: "WorkTaskStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTaskStatusTranslations_WorkTaskStatusId_Language",
                table: "WorkTaskStatusTranslations",
                columns: new[] { "WorkTaskStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTickets_AssigneeId",
                table: "WorkTickets",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTickets_Code",
                table: "WorkTickets",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTickets_PriorityId",
                table: "WorkTickets",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTickets_StatusId",
                table: "WorkTickets",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTickets_WorkGroupId",
                table: "WorkTickets",
                column: "WorkGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTickets_WorkProjectId",
                table: "WorkTickets",
                column: "WorkProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTickets_WorkTicketStatusId",
                table: "WorkTickets",
                column: "WorkTicketStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTickets_WorkTicketTypeId",
                table: "WorkTickets",
                column: "WorkTicketTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTicketStatuses_Code",
                table: "WorkTicketStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTicketStatusTranslations_WorkTicketStatusId_Language",
                table: "WorkTicketStatusTranslations",
                columns: new[] { "WorkTicketStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTicketTypes_Code",
                table: "WorkTicketTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTicketTypeTranslations_WorkTicketTypeId_Language",
                table: "WorkTicketTypeTranslations",
                columns: new[] { "WorkTicketTypeId", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionTranslations");

            migrationBuilder.DropTable(
                name: "ProjectKindTranslations");

            migrationBuilder.DropTable(
                name: "ProjectStatusTranslations");

            migrationBuilder.DropTable(
                name: "ProjectTypeTranslations");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "WorkGroupStatusTranslations");

            migrationBuilder.DropTable(
                name: "WorkItemPriorityTranslations");

            migrationBuilder.DropTable(
                name: "WorkProjectParticipantRoles");

            migrationBuilder.DropTable(
                name: "WorkTasks");

            migrationBuilder.DropTable(
                name: "WorkTaskStatusTranslations");

            migrationBuilder.DropTable(
                name: "WorkTicketStatusTranslations");

            migrationBuilder.DropTable(
                name: "WorkTicketTypeTranslations");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "WorkTickets");

            migrationBuilder.DropTable(
                name: "WorkGroups");

            migrationBuilder.DropTable(
                name: "WorkItemPriorities");

            migrationBuilder.DropTable(
                name: "WorkProjectParticipants");

            migrationBuilder.DropTable(
                name: "WorkTaskStatuses");

            migrationBuilder.DropTable(
                name: "WorkTicketStatuses");

            migrationBuilder.DropTable(
                name: "WorkTicketTypes");

            migrationBuilder.DropTable(
                name: "WorkGroupStatuses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WorkProjects");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "ProjectKinds");

            migrationBuilder.DropTable(
                name: "ProjectStatuses");

            migrationBuilder.DropTable(
                name: "ProjectTypes");
        }
    }
}
