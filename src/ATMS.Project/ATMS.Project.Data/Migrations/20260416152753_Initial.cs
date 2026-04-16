using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                    LogoPath = table.Column<string>(type: "text", nullable: true, defaultValue: "default-org.png"),
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
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "ProjectKindTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectKindId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectKindTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectKindTranslation_ProjectKinds_ProjectKindId",
                        column: x => x.ProjectKindId,
                        principalTable: "ProjectKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatusTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatusTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectStatusTranslation_ProjectStatuses_ProjectStatusId",
                        column: x => x.ProjectStatusId,
                        principalTable: "ProjectStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTypeTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ProjectTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTypeTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTypeTranslation_ProjectTypes_ProjectTypeId",
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
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectTypeId = table.Column<int>(type: "integer", nullable: false),
                    ProjectKindId = table.Column<int>(type: "integer", nullable: false),
                    ProjectStatusId = table.Column<int>(type: "integer", nullable: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "WorkGroupStatusTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkGroupStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkGroupStatusTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkGroupStatusTranslation_WorkGroupStatuses_WorkGroupStatu~",
                        column: x => x.WorkGroupStatusId,
                        principalTable: "WorkGroupStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemPriorityTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkItemPriorityId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemPriorityTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItemPriorityTranslation_WorkItemPriorities_WorkItemPrio~",
                        column: x => x.WorkItemPriorityId,
                        principalTable: "WorkItemPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTaskStatusTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkTaskStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTaskStatusTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTaskStatusTranslation_WorkTaskStatuses_WorkTaskStatusId",
                        column: x => x.WorkTaskStatusId,
                        principalTable: "WorkTaskStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTicketStatusTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkTicketStatusId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTicketStatusTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTicketStatusTranslation_WorkTicketStatuses_WorkTicketSt~",
                        column: x => x.WorkTicketStatusId,
                        principalTable: "WorkTicketStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTicketTypeTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkTicketTypeId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTicketTypeTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTicketTypeTranslation_WorkTicketTypes_WorkTicketTypeId",
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkProjectParticipants_WorkProjects_WorkProjectId",
                        column: x => x.WorkProjectId,
                        principalTable: "WorkProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkProjectParticipantRoles_WorkProjectParticipants_WorkPro~",
                        column: x => x.WorkProjectParticipantId,
                        principalTable: "WorkProjectParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "ProjectView" },
                    { 2, "ProjectEdit" },
                    { 3, "ProjectDelete" },
                    { 4, "TicketView" },
                    { 5, "TicketEdit" },
                    { 6, "TicketDelete" },
                    { 7, "TaskView" },
                    { 8, "TaskEdit" },
                    { 9, "TaskDelete" },
                    { 10, "CommentView" },
                    { 11, "CommentEdit" },
                    { 12, "CommentDelete" },
                    { 13, "NotificationView" },
                    { 14, "NotificationEdit" },
                    { 15, "NotificationDelete" },
                    { 16, "GroupView" },
                    { 17, "GroupEdit" },
                    { 18, "GroupDelete" },
                    { 19, "DictionaryView" },
                    { 20, "DictionaryEdit" },
                    { 21, "DictionaryDelete" },
                    { 22, "OrganizationView" },
                    { 23, "OrganizationEdit" },
                    { 24, "OrganizationDelete" },
                    { 25, "UserView" },
                    { 26, "UserEdit" },
                    { 27, "UserDelete" },
                    { 28, "UserInvite" }
                });

            migrationBuilder.InsertData(
                table: "ProjectKinds",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "Support" },
                    { 2, "External" },
                    { 3, "Internal" },
                    { 4, "OneTime" }
                });

            migrationBuilder.InsertData(
                table: "ProjectStatuses",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "Draft" },
                    { 2, "Active" },
                    { 3, "OnReview" },
                    { 4, "Closed" }
                });

            migrationBuilder.InsertData(
                table: "ProjectTypes",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "Standard" },
                    { 2, "Optimal" },
                    { 3, "Premium" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "DeletedAt", "DeletedById", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("51805e71-420c-40c4-a074-76b4f29eee7a"), null, null, "Developer Role", "Developer" },
                    { new Guid("6b738142-0c09-47d0-848b-f2d5e411b266"), null, null, "Client Viewer Role", "Client Viewer" },
                    { new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca"), null, null, "Business Consultant Role", "Business Consultant" },
                    { new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890"), null, null, "Project Manager Role", "Project Manager" },
                    { new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346"), null, null, "Client Manager Role", "Client Manager" }
                });

            migrationBuilder.InsertData(
                table: "WorkGroupStatuses",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "Planned" },
                    { 2, "Active" },
                    { 3, "Done" }
                });

            migrationBuilder.InsertData(
                table: "WorkItemPriorities",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "Low" },
                    { 2, "Medium" },
                    { 3, "High" }
                });

            migrationBuilder.InsertData(
                table: "WorkTaskStatuses",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "New" },
                    { 2, "InProgress" },
                    { 3, "Done" }
                });

            migrationBuilder.InsertData(
                table: "WorkTicketStatuses",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "New" },
                    { 2, "InProgress" },
                    { 3, "InReview" },
                    { 4, "Testing" },
                    { 5, "Closed" },
                    { 6, "Rejected" }
                });

            migrationBuilder.InsertData(
                table: "WorkTicketTypes",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { 1, "Bug" },
                    { 2, "Feature" },
                    { 3, "Task" }
                });

            migrationBuilder.InsertData(
                table: "PermissionTranslation",
                columns: new[] { "Id", "Language", "Name", "PermissionId" },
                values: new object[,]
                {
                    { 1, "en", "Project view", 1 },
                    { 2, "ru", "Просмотр проектов", 1 },
                    { 3, "az", "Layihəyə baxış", 1 },
                    { 4, "en", "Project edit", 2 },
                    { 5, "ru", "Редактирование проектов", 2 },
                    { 6, "az", "Layihəni redaktə et", 2 },
                    { 7, "en", "Project delete", 3 },
                    { 8, "ru", "Удаление проектов", 3 },
                    { 9, "az", "Layihəni sil", 3 },
                    { 10, "en", "Ticket view", 4 },
                    { 11, "ru", "Просмотр тикетов", 4 },
                    { 12, "az", "Tiketi baxış", 4 },
                    { 13, "en", "Ticket edit", 5 },
                    { 14, "ru", "Редактирование тикетов", 5 },
                    { 15, "az", "Tiketi redaktə et", 5 },
                    { 16, "en", "Ticket delete", 6 },
                    { 17, "ru", "Удаление тикетов", 6 },
                    { 18, "az", "Tiketi sil", 6 },
                    { 19, "en", "Task view", 7 },
                    { 20, "ru", "Просмотр задач", 7 },
                    { 21, "az", "Tapşırığa baxış", 7 },
                    { 22, "en", "Task edit", 8 },
                    { 23, "ru", "Редактирование задач", 8 },
                    { 24, "az", "Tapşırığı redaktə et", 8 },
                    { 25, "en", "Task delete", 9 },
                    { 26, "ru", "Удаление задач", 9 },
                    { 27, "az", "Tapşırığı sil", 9 },
                    { 28, "en", "Comment view", 10 },
                    { 29, "ru", "Просмотр комментариев", 10 },
                    { 30, "az", "Şərhə baxış", 10 },
                    { 31, "en", "Comment edit", 11 },
                    { 32, "ru", "Редактирование комментариев", 11 },
                    { 33, "az", "Şərhi redaktə et", 11 },
                    { 34, "en", "Comment delete", 12 },
                    { 35, "ru", "Удаление комментариев", 12 },
                    { 36, "az", "Şərhi sil", 12 },
                    { 37, "en", "Notification view", 13 },
                    { 38, "ru", "Просмотр уведомлений", 13 },
                    { 39, "az", "Bildirişə baxış", 13 },
                    { 40, "en", "Notification edit", 14 },
                    { 41, "ru", "Редактирование уведомлений", 14 },
                    { 42, "az", "Bildirişi redaktə et", 14 },
                    { 43, "en", "Notification delete", 15 },
                    { 44, "ru", "Удаление уведомлений", 15 },
                    { 45, "az", "Bildirişi sil", 15 },
                    { 46, "en", "Group view", 16 },
                    { 47, "ru", "Просмотр групп", 16 },
                    { 48, "az", "Qrupa baxış", 16 },
                    { 49, "en", "Group edit", 17 },
                    { 50, "ru", "Редактирование групп", 17 },
                    { 51, "az", "Qrupu redaktə et", 17 },
                    { 52, "en", "Group delete", 18 },
                    { 53, "ru", "Удаление групп", 18 },
                    { 54, "az", "Qrupu sil", 18 },
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
                    { 75, "az", "İstifadəçiyə baxış", 25 },
                    { 76, "en", "User edit", 26 },
                    { 77, "ru", "Редактирование польз.", 26 },
                    { 78, "az", "İstifadəçini redaktə", 26 },
                    { 79, "en", "User delete", 27 },
                    { 80, "ru", "Удаление пользователей", 27 },
                    { 81, "az", "İstifadəçини sil", 27 }
                });

            migrationBuilder.InsertData(
                table: "ProjectKindTranslation",
                columns: new[] { "Id", "Language", "Name", "ProjectKindId" },
                values: new object[,]
                {
                    { 1, "en", "Support", 1 },
                    { 2, "ru", "Поддержка", 1 },
                    { 3, "az", "Dəstək", 1 },
                    { 4, "en", "External", 2 },
                    { 5, "ru", "Внешний", 2 },
                    { 6, "az", "Xarici", 2 },
                    { 7, "en", "Internal", 3 },
                    { 8, "ru", "Внутренний", 3 },
                    { 9, "az", "Daxili", 3 },
                    { 10, "en", "One Time", 4 },
                    { 11, "ru", "Разовый", 4 },
                    { 12, "az", "Birdəfəlik", 4 }
                });

            migrationBuilder.InsertData(
                table: "ProjectStatusTranslation",
                columns: new[] { "Id", "Language", "Name", "ProjectStatusId" },
                values: new object[,]
                {
                    { 1, "en", "Draft", 1 },
                    { 2, "ru", "Черновик", 1 },
                    { 3, "az", "Qaralama", 1 },
                    { 4, "en", "Active", 2 },
                    { 5, "ru", "Активный", 2 },
                    { 6, "az", "Aktiv", 2 },
                    { 7, "en", "In Review", 3 },
                    { 8, "ru", "На проверке", 3 },
                    { 9, "az", "Yoxlamada", 3 },
                    { 10, "en", "Closed", 4 },
                    { 11, "ru", "Закрыт", 4 },
                    { 12, "az", "Bağlanıb", 4 }
                });

            migrationBuilder.InsertData(
                table: "ProjectTypeTranslation",
                columns: new[] { "Id", "Language", "Name", "ProjectTypeId" },
                values: new object[,]
                {
                    { 1, "en", "Standard", 1 },
                    { 2, "ru", "Стандартный", 1 },
                    { 3, "az", "Standart", 1 },
                    { 4, "en", "Optimal", 2 },
                    { 5, "ru", "Оптимальный", 2 },
                    { 6, "az", "Optimal", 2 },
                    { 7, "en", "Premium", 3 },
                    { 8, "ru", "Премиум", 3 },
                    { 9, "az", "Premium", 3 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 1, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 1, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 1, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 1, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 2, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 2, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 2, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 3, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 3, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 3, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 4, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 4, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 4, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 4, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 4, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 5, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 5, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 5, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 6, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 6, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 6, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 7, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 7, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 7, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 7, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 7, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 8, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 8, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 8, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 9, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 9, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 9, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 10, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 10, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 10, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 10, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 10, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 11, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 11, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 11, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 11, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 11, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 12, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 12, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 12, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 12, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 12, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 13, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 13, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 13, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 13, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 13, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 16, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 16, new Guid("6b738142-0c09-47d0-848b-f2d5e411b266") },
                    { 16, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 16, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 16, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") },
                    { 17, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 17, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 17, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 18, new Guid("51805e71-420c-40c4-a074-76b4f29eee7a") },
                    { 18, new Guid("7b59a306-3455-4d35-bb7d-d7a07e8219ca") },
                    { 18, new Guid("869cbfbe-f0ad-4357-b369-71b3ece4a890") },
                    { 28, new Guid("fa1dac7e-d57c-4e4c-9f71-283566862346") }
                });

            migrationBuilder.InsertData(
                table: "WorkGroupStatusTranslation",
                columns: new[] { "Id", "Language", "Name", "WorkGroupStatusId" },
                values: new object[,]
                {
                    { 1, "en", "Planned", 1 },
                    { 2, "ru", "Запланировано", 1 },
                    { 3, "az", "Planlaşdırılıb", 1 },
                    { 4, "en", "Active", 2 },
                    { 5, "ru", "Активный", 2 },
                    { 6, "az", "Aktiv", 2 },
                    { 7, "en", "Done", 3 },
                    { 8, "ru", "Завершено", 3 },
                    { 9, "az", "Bitdi", 3 }
                });

            migrationBuilder.InsertData(
                table: "WorkItemPriorityTranslation",
                columns: new[] { "Id", "Language", "Name", "WorkItemPriorityId" },
                values: new object[,]
                {
                    { 1, "en", "Low", 1 },
                    { 2, "ru", "Низкий", 1 },
                    { 3, "az", "Aşağı", 1 },
                    { 4, "en", "Medium", 2 },
                    { 5, "ru", "Средний", 2 },
                    { 6, "az", "Orta", 2 },
                    { 7, "en", "High", 3 },
                    { 8, "ru", "Высокий", 3 },
                    { 9, "az", "Yüksək", 3 }
                });

            migrationBuilder.InsertData(
                table: "WorkTaskStatusTranslation",
                columns: new[] { "Id", "Language", "Name", "WorkTaskStatusId" },
                values: new object[,]
                {
                    { 1, "en", "New", 1 },
                    { 2, "ru", "Новый", 1 },
                    { 3, "az", "Yeni", 1 },
                    { 4, "en", "In Progress", 2 },
                    { 5, "ru", "В работе", 2 },
                    { 6, "az", "İşdə", 2 },
                    { 7, "en", "Done", 3 },
                    { 8, "ru", "Выполнено", 3 },
                    { 9, "az", "Hazır", 3 }
                });

            migrationBuilder.InsertData(
                table: "WorkTicketStatusTranslation",
                columns: new[] { "Id", "Language", "Name", "WorkTicketStatusId" },
                values: new object[,]
                {
                    { 1, "en", "New", 1 },
                    { 2, "ru", "Новый", 1 },
                    { 3, "az", "Yeni", 1 },
                    { 4, "en", "In Progress", 2 },
                    { 5, "ru", "В работе", 2 },
                    { 6, "az", "İşdə", 2 },
                    { 7, "en", "In Review", 3 },
                    { 8, "ru", "На проверке", 3 },
                    { 9, "az", "Yoxlamada", 3 },
                    { 10, "en", "Testing", 4 },
                    { 11, "ru", "Тестирование", 4 },
                    { 12, "az", "Test mərhələsində", 4 },
                    { 13, "en", "Closed", 5 },
                    { 14, "ru", "Закрыт", 5 },
                    { 15, "az", "Bağlandı", 5 },
                    { 16, "en", "Rejected", 6 },
                    { 17, "ru", "Отклонён", 6 },
                    { 18, "az", "Rədd edildi", 6 }
                });

            migrationBuilder.InsertData(
                table: "WorkTicketTypeTranslation",
                columns: new[] { "Id", "Language", "Name", "WorkTicketTypeId" },
                values: new object[,]
                {
                    { 1, "en", "Bug", 1 },
                    { 2, "ru", "Ошибка", 1 },
                    { 3, "az", "Xəta", 1 },
                    { 4, "en", "Feature", 2 },
                    { 5, "ru", "Новая функция", 2 },
                    { 6, "az", "Təzə Funksiya", 2 },
                    { 7, "en", "Task", 3 },
                    { 8, "ru", "Задача", 3 },
                    { 9, "az", "Tapşırıq", 3 }
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
                name: "IX_PermissionTranslation_PermissionId_Language",
                table: "PermissionTranslation",
                columns: new[] { "PermissionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectKinds_Code",
                table: "ProjectKinds",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectKindTranslation_ProjectKindId_Language",
                table: "ProjectKindTranslation",
                columns: new[] { "ProjectKindId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatuses_Code",
                table: "ProjectStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatusTranslation_ProjectStatusId_Language",
                table: "ProjectStatusTranslation",
                columns: new[] { "ProjectStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTypes_Code",
                table: "ProjectTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTypeTranslation_ProjectTypeId_Language",
                table: "ProjectTypeTranslation",
                columns: new[] { "ProjectTypeId", "Language" },
                unique: true);

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
                name: "IX_WorkGroupStatusTranslation_WorkGroupStatusId_Language",
                table: "WorkGroupStatusTranslation",
                columns: new[] { "WorkGroupStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemPriorities_Code",
                table: "WorkItemPriorities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemPriorityTranslation_WorkItemPriorityId_Language",
                table: "WorkItemPriorityTranslation",
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
                name: "IX_WorkTaskStatusTranslation_WorkTaskStatusId_Language",
                table: "WorkTaskStatusTranslation",
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
                name: "IX_WorkTicketStatusTranslation_WorkTicketStatusId_Language",
                table: "WorkTicketStatusTranslation",
                columns: new[] { "WorkTicketStatusId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTicketTypes_Code",
                table: "WorkTicketTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTicketTypeTranslation_WorkTicketTypeId_Language",
                table: "WorkTicketTypeTranslation",
                columns: new[] { "WorkTicketTypeId", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionTranslation");

            migrationBuilder.DropTable(
                name: "ProjectKindTranslation");

            migrationBuilder.DropTable(
                name: "ProjectStatusTranslation");

            migrationBuilder.DropTable(
                name: "ProjectTypeTranslation");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "WorkGroupStatusTranslation");

            migrationBuilder.DropTable(
                name: "WorkItemPriorityTranslation");

            migrationBuilder.DropTable(
                name: "WorkProjectParticipantRoles");

            migrationBuilder.DropTable(
                name: "WorkTasks");

            migrationBuilder.DropTable(
                name: "WorkTaskStatusTranslation");

            migrationBuilder.DropTable(
                name: "WorkTicketStatusTranslation");

            migrationBuilder.DropTable(
                name: "WorkTicketTypeTranslation");

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
