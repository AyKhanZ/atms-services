using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedSomeFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FailedLoginCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UserStatusId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "UserStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserStatusId",
                table: "Users",
                column: "UserStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatuses_Code",
                table: "UserStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.InsertData(
                table: "UserStatuses",
                columns: new[] { "Id", "Name", "Code" },
                values: new object[,]
                {
                    { 1, "Active", "Active" },
                    { 2, "Locked", "Locked" },
                    { 3, "Deleted", "Deleted" }
                });

            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""UserStatusId"" = 1,
                    ""FailedLoginCount"" = 0,
                    ""LockoutEnd"" = '0001-01-01T00:00:00Z'
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserStatuses_UserStatusId",
                table: "Users",
                column: "UserStatusId",
                principalTable: "UserStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserStatuses_UserStatusId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserStatusId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FailedLoginCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserStatusId",
                table: "Users");
        }
    }
}
