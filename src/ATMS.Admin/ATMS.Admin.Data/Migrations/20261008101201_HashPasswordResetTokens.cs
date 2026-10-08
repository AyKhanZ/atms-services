using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class HashPasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // old links live 1 hour and their raw value cannot be turned into a hash, so they are dropped
            migrationBuilder.Sql("DELETE FROM \"PasswordResetTokens\";");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "PasswordResetTokens");

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "PasswordResetTokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_TokenHash",
                table: "PasswordResetTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // a hash cannot be turned back into a link, so the links are dropped
            migrationBuilder.Sql("DELETE FROM \"PasswordResetTokens\";");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_TokenHash",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "PasswordResetTokens");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "PasswordResetTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);
        }
    }
}
