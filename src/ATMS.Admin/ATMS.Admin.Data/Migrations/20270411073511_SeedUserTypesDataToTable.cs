using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUserTypesDataToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserTypes",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "Agent"   },
                    { 2, "Client" }
                });

            migrationBuilder.InsertData(
                table: "UserTypeTranslations",
                columns: ["UserTypeId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Client"       },
                    { 1, "ru", "Клиент"       },
                    { 1, "az", "Müştəri"      },
                    { 2, "en", "Agent"        },
                    { 2, "ru", "Агент"        },
                    { 2, "az", "Agent"        }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "UserTypes", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "UserTypes", keyColumn: "Id", keyValue: 2);
        }
    }
}
