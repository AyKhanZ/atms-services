using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations;

public partial class SeedMaritalStatusesDataToTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "MaritalStatuses",
            columns: ["Id", "Code"],
            columnTypes: ["integer", "character varying(50)"],
            values: new object[,]
            {
                { 1, "NotSpecified" },
                { 2, "Single"       },
                { 3, "Married"      }
            });

        migrationBuilder.InsertData(
            table: "MaritalStatusTranslations",
            columns: ["MaritalStatusId", "Language", "Name"],
            columnTypes: ["integer", "character varying(5)", "character varying(100)"],
            values: new object[,]
            {
                { 1, "en", "Not specified"  },
                { 1, "ru", "Не указано"     },
                { 1, "az", "Göstərilməyib"  },
                { 2, "en", "Single"         },
                { 2, "ru", "Холост"         },
                { 2, "az", "Subay"          },
                { 3, "en", "Married"        },
                { 3, "ru", "Женат"          },
                { 3, "az", "Evli"           }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(table: "MaritalStatuses", keyColumn: "Id", keyValue: 1);
        migrationBuilder.DeleteData(table: "MaritalStatuses", keyColumn: "Id", keyValue: 2);
        migrationBuilder.DeleteData(table: "MaritalStatuses", keyColumn: "Id", keyValue: 3);
    }
}
