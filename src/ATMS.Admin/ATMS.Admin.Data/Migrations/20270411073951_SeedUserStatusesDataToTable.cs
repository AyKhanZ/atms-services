using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUserStatusesDataToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserStatuses",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "Active"   },
                    { 2, "Inactive" },
                    { 3, "Locked"   }
                });

            migrationBuilder.InsertData(
                table: "UserStatusTranslations",
                columns: ["UserStatusId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Active"       },
                    { 1, "ru", "Активный"     },
                    { 1, "az", "Aktiv"        },
                    { 2, "en", "Inactive"     },
                    { 2, "ru", "Неактивный"   },
                    { 2, "az", "Qeyri-aktiv"  },
                    { 3, "en", "Locked"       },
                    { 3, "ru", "Заблокирован" },
                    { 3, "az", "Bloklanmış"   }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "UserStatuses", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "UserStatuses", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "UserStatuses", keyColumn: "Id", keyValue: 3);
        }
    }
}
