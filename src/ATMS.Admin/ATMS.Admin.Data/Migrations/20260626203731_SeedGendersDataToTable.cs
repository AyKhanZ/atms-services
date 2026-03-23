using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations;

public partial class SeedGendersDataToTable : Migration
{
    private readonly List<(int Id, string Name, string Code)> items =
    [
        (1, "Not specified", "NotSpecified"),
        (2, "Male", "Male"),
        (3, "Female", "Female"),
        (4, "Other", "Other")
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var item in items)
        {
            migrationBuilder.InsertData(
                table: "Genders",
                columns: ["Id", "Name", "Code"],
                values: [item.Id, item.Name, item.Code]);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var id in items.Select(x => x.Id))
        {
            migrationBuilder.DeleteData(
                table: "Genders",
                keyColumn: "Id",
                keyValue: id);
        }
    }
}
