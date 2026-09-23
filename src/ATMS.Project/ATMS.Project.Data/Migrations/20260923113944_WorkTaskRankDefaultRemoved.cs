using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkTaskRankDefaultRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The empty default was only there to fill the column when it was added. Leaving it in
            // place would let a row without a position be saved, and an empty key sorts to the top
            // of every column.
            migrationBuilder.Sql("""ALTER TABLE "Tasks" ALTER COLUMN "Rank" DROP DEFAULT;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "Tasks" ALTER COLUMN "Rank" SET DEFAULT '';""");
        }
    }
}
