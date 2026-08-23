using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkItemConstraintsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectGroups_Code",
                table: "ProjectGroups");

            migrationBuilder.DropIndex(
                name: "IX_ProjectGroups_Title",
                table: "ProjectGroups");

            migrationBuilder.DropIndex(
                name: "IX_ProjectGroups_WorkProjectId_ParentWorkGroupId_Title",
                table: "ProjectGroups");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectGroups_Level",
                table: "ProjectGroups");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProjectGroups");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ProjectGroups");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tickets",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ProjectGroups",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_WorkProjectId_ParentWorkGroupId_Title",
                table: "ProjectGroups",
                columns: new[] { "WorkProjectId", "ParentWorkGroupId", "Title" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectGroups_WorkProjectId_ParentWorkGroupId_Title",
                table: "ProjectGroups");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tickets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ProjectGroups",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ProjectGroups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Level",
                table: "ProjectGroups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_Code",
                table: "ProjectGroups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_Title",
                table: "ProjectGroups",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_WorkProjectId_ParentWorkGroupId_Title",
                table: "ProjectGroups",
                columns: new[] { "WorkProjectId", "ParentWorkGroupId", "Title" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectGroups_Level",
                table: "ProjectGroups",
                sql: "\"Level\" <= 1");
        }
    }
}
