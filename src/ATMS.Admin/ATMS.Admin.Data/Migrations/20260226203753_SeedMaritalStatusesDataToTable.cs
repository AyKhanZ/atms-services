using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations;

public partial class SeedMaritalStatusesDataToTable : Migration
{
    private readonly List<(int Id, string Name, string Code)> items =
    [
        (1, "Not specified", "NotSpecified" ),
        (2, "Single", "Single" ),
        (3, "Married", "Married" ),
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var item in items)
        {
            migrationBuilder.InsertData(
                table: "MaritalStatuses",
                columns: ["Id", "Name", "Code"],
                values: [item.Id, item.Name, item.Code]);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var id in items.Select(x => x.Id))
        {
            migrationBuilder.DeleteData(
                table: "MaritalStatuses",
                keyColumn: "Id",
                keyValue: id);
        }
    }
}
