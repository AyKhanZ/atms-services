using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditableEntityUpdatedByAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "Tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "ProjectGroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "Organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "Meetings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "MeetingMinutes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "Attachments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UpdatedById",
                table: "Projects",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_UpdatedById",
                table: "Projects",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_UpdatedById",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_UpdatedById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "ProjectGroups");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "MeetingMinutes");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Attachments");
        }
    }
}
