using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedGendersDataToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Genders",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "NotSpecified" },
                    { 2, "Male"         },
                    { 3, "Female"       },
                    { 4, "Other"        }
                });

            migrationBuilder.InsertData(
                table: "GenderTranslations",
                columns: ["GenderId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Not specified"  },
                    { 1, "ru", "Не указано"     },
                    { 1, "az", "Göstərilməyib"  },
                    { 2, "en", "Male"           },
                    { 2, "ru", "Мужской"        },
                    { 2, "az", "Kişi"           },
                    { 3, "en", "Female"         },
                    { 3, "ru", "Женский"        },
                    { 3, "az", "Qadın"          },
                    { 4, "en", "Other"          },
                    { 4, "ru", "Другое"         },
                    { 4, "az", "Digər"          }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Genders", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Genders", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "Genders", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "Genders", keyColumn: "Id", keyValue: 4);
        }
    }
}
