using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkProjectSoftDeletionAddedAndMaxLenChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId_Title",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Title",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectParticipants_WorkProjectId_UserId",
                table: "ProjectParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ProjectParticipantRoles_WorkProjectParticipantId_RoleId",
                table: "ProjectParticipantRoles");

            migrationBuilder.CreateSequence(
                name: "EntityCodeSequence");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Projects",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EndDate",
                table: "Projects",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_Title",
                table: "Projects",
                columns: new[] { "OrganizationId", "Title" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StartDate",
                table: "Projects",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipants_WorkProjectId_UserId",
                table: "ProjectParticipants",
                columns: new[] { "WorkProjectId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipantRoles_WorkProjectParticipantId_RoleId",
                table: "ProjectParticipantRoles",
                columns: new[] { "WorkProjectParticipantId", "RoleId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_EndDate",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId_Title",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_StartDate",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectParticipants_WorkProjectId_UserId",
                table: "ProjectParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ProjectParticipantRoles_WorkProjectParticipantId_RoleId",
                table: "ProjectParticipantRoles");

            migrationBuilder.DropSequence(
                name: "EntityCodeSequence");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Projects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_Title",
                table: "Projects",
                columns: new[] { "OrganizationId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Title",
                table: "Projects",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipants_WorkProjectId_UserId",
                table: "ProjectParticipants",
                columns: new[] { "WorkProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParticipantRoles_WorkProjectParticipantId_RoleId",
                table: "ProjectParticipantRoles",
                columns: new[] { "WorkProjectParticipantId", "RoleId" },
                unique: true);
        }
    }
}
