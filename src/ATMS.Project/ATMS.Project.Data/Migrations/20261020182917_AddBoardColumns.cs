using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_StatusId_Rank",
                table: "Tasks");

            migrationBuilder.CreateTable(
                name: "BoardColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardColumnTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoardColumnId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumnTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardColumnTranslations_BoardColumns_BoardColumnId",
                        column: x => x.BoardColumnId,
                        principalTable: "BoardColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BoardColumns",
                columns: new[] { "Id", "Category", "Code", "Order" },
                values: new object[,]
                {
                    { 1, 1, "New", 1 },
                    { 2, 2, "InProgressDoing", 2 },
                    { 3, 3, "Done", 4 },
                    { 4, 2, "InProgressDone", 3 }
                });

            migrationBuilder.InsertData(
                table: "BoardColumnTranslations",
                columns: new[] { "Id", "BoardColumnId", "Language", "Name" },
                values: new object[,]
                {
                    { 1, 1, "en", "New" },
                    { 2, 1, "ru", "Новый" },
                    { 3, 1, "az", "Yeni" },
                    { 4, 2, "en", "Doing" },
                    { 5, 2, "ru", "В процессе" },
                    { 6, 2, "az", "İcrada" },
                    { 7, 4, "en", "Done" },
                    { 8, 4, "ru", "Готово" },
                    { 9, 4, "az", "Bitib" },
                    { 10, 3, "en", "Done" },
                    { 11, 3, "ru", "Выполнено" },
                    { 12, 3, "az", "Hazır" }
                });

            // existing tasks keep their status: columns 1-3 carry the ids of New, In Progress and Done
            migrationBuilder.AddColumn<int>(
                name: "BoardColumnId",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Tasks" SET "BoardColumnId" = "StatusId";
                """);

            migrationBuilder.AlterColumn<int>(
                name: "BoardColumnId",
                table: "Tasks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_BoardColumnId",
                table: "Tasks",
                column: "BoardColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_BoardColumnId_Rank",
                table: "Tasks",
                columns: new[] { "BoardColumnId", "Rank" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"StatusId\" <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_Code",
                table: "BoardColumns",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_Order",
                table: "BoardColumns",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumnTranslations_BoardColumnId_Language",
                table: "BoardColumnTranslations",
                columns: new[] { "BoardColumnId", "Language" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_BoardColumns_BoardColumnId",
                table: "Tasks",
                column: "BoardColumnId",
                principalTable: "BoardColumns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // renumbering below would collide row by row with the per-column index, drop it first
            migrationBuilder.DropIndex(
                name: "IX_Tasks_BoardColumnId_Rank",
                table: "Tasks");

            // the rank was unique per column and must be unique per status again (Doing and Done of
            // In Progress merge): renumber each status top down, Doing above Done, as RenumberColumnAsync does
            migrationBuilder.Sql("""
                UPDATE "Tasks" AS t
                SET "Rank" = 'm' || lpad((ordered.n * 1000)::text, 12, '0') || 'v'
                FROM (
                    SELECT task."Id", row_number() OVER (
                        PARTITION BY task."StatusId" ORDER BY c."Order", task."Rank", task."Id") AS n
                    FROM "Tasks" AS task
                    JOIN "BoardColumns" AS c ON c."Id" = task."BoardColumnId"
                    WHERE NOT task."IsDeleted" AND task."StatusId" <> 3
                ) AS ordered
                WHERE ordered."Id" = t."Id";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_BoardColumns_BoardColumnId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "BoardColumnTranslations");

            migrationBuilder.DropTable(
                name: "BoardColumns");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_BoardColumnId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "BoardColumnId",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StatusId_Rank",
                table: "Tasks",
                columns: new[] { "StatusId", "Rank" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"StatusId\" <> 3");
        }
    }
}
