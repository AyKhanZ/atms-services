using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermissionsDataToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: ["Id", "Code", "Module"],
                columnTypes: ["integer", "character varying(50)", "character varying(50)"],
                values: new object[,]
                {
                    {  1, "RoleView",           "Role"         },
                    {  2, "RoleEdit",           "Role"         },
                    {  3, "RoleDelete",         "Role"         },
                    {  4, "UserView",           "User"         },
                    {  5, "UserEdit",           "User"         },
                    {  6, "UserDelete",         "User"         },
                    {  7, "ProjectView",        "Project"      },
                    {  8, "ProjectEdit",        "Project"      },
                    {  9, "ProjectDelete",      "Project"      },
                    { 10, "CommentView",        "Comment"      },
                    { 11, "CommentEdit",        "Comment"      },
                    { 12, "CommentDelete",      "Comment"      },
                    { 13, "NotificationView",   "Notification" },
                    { 14, "NotificationEdit",   "Notification" },
                    { 15, "NotificationDelete", "Notification" }
                });

            migrationBuilder.InsertData(
                table: "PermissionTranslations",
                columns: ["PermissionId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    {  1, "en", "Role view"              },
                    {  1, "ru", "Просмотр ролей"         },
                    {  1, "az", "Rola baxış"             },
                    {  2, "en", "Role edit"              },
                    {  2, "ru", "Редактирование ролей"   },
                    {  2, "az", "Rolu redaktə et"        },
                    {  3, "en", "Role delete"            },
                    {  3, "ru", "Удаление ролей"         },
                    {  3, "az", "Rolu sil"               },
                    {  4, "en", "User view"              },
                    {  4, "ru", "Просмотр пользователей" },
                    {  4, "az", "İstifadəçiyə baxış"     },
                    {  5, "en", "User edit"              },
                    {  5, "ru", "Редактирование польз."  },
                    {  5, "az", "İstifadəçini redaktə"   },
                    {  6, "en", "User delete"            },
                    {  6, "ru", "Удаление пользователей" },
                    {  6, "az", "İstifadəçini sil"       },
                    {  7, "en", "Project view"           },
                    {  7, "ru", "Просмотр проектов"      },
                    {  7, "az", "Layihəyə baxış"         },
                    {  8, "en", "Project edit"           },
                    {  8, "ru", "Редактирование проектов"},
                    {  8, "az", "Layihəni redaktə"       },
                    {  9, "en", "Project delete"         },
                    {  9, "ru", "Удаление проектов"      },
                    {  9, "az", "Layihəni sil"           },
                    { 10, "en", "Comment view"           },
                    { 10, "ru", "Просмотр комментариев"  },
                    { 10, "az", "Şərhə baxış"            },
                    { 11, "en", "Comment edit"           },
                    { 11, "ru", "Редактирование коммент."},
                    { 11, "az", "Şərhi redaktə"          },
                    { 12, "en", "Comment delete"         },
                    { 12, "ru", "Удаление комментариев"  },
                    { 12, "az", "Şərhi sil"              },
                    { 13, "en", "Notification view"      },
                    { 13, "ru", "Просмотр уведомлений"   },
                    { 13, "az", "Bildirişə baxış"         },
                    { 14, "en", "Notification edit"      },
                    { 14, "ru", "Редактирование уведомл."},
                    { 14, "az", "Bildirişi redaktə"       },
                    { 15, "en", "Notification delete"    },
                    { 15, "ru", "Удаление уведомлений"   },
                    { 15, "az", "Bildirişi sil"           }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (var id = 1; id <= 15; id++)
                migrationBuilder.DeleteData(table: "Permissions", keyColumn: "Id", keyValue: id);
        }
    }
}
