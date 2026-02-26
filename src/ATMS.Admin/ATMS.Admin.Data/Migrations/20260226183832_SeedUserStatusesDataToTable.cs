using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations;

public partial class SeedUserStatusesDataToTable : Migration
{
    private readonly List<(int Id, string Name, string Code)> items =
    [
        (1, "Active", "Active"),
        (2, "Inactive", "Inactive"),
        (3, "Locked", "Locked")
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var item in items)
        {
            migrationBuilder.InsertData(
                table: "UserStatuses",
                columns: ["Id", "Name", "Code"],
                values: [item.Id, item.Name, item.Code]);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        //migrationBuilder.Sql("DELETE FROM UserStatuses");
        foreach (var id in items.Select(x => x.Id))
        {
            migrationBuilder.DeleteData(
                table: "UserStatuses",
                keyColumn: "Id",
                keyValue: id);
        }
    }
}
