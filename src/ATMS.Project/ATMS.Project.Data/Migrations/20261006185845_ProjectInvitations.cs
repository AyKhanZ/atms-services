using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only Admin knows who finished onboarding. Rows that exist before this release are taken
            // as finished, so nobody old shows the Invited status; the default is dropped right after,
            // and every later value comes from the user events.
            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedOnboarding",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"HasCompletedOnboarding\" DROP DEFAULT;");

            // Existing rows take the email Admin normalised the same way; later rows are filled by the
            // user created event mapping.
            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"Users\" SET \"NormalizedEmail\" = UPPER(TRIM(\"Email\"));");
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"NormalizedEmail\" DROP DEFAULT;");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RoutingKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MessageType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InvitedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Projects_WorkProjectId",
                        column: x => x.WorkProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Users_InvitedById",
                        column: x => x.InvitedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextAttemptAt",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_InvitedById",
                table: "ProjectInvitations",
                column: "InvitedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_NormalizedEmail",
                table: "ProjectInvitations",
                column: "NormalizedEmail",
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_RoleId",
                table: "ProjectInvitations",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_WorkProjectId_NormalizedEmail",
                table: "ProjectInvitations",
                columns: new[] { "WorkProjectId", "NormalizedEmail" },
                filter: "\"Status\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "ProjectInvitations");

            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HasCompletedOnboarding",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");
        }
    }
}
