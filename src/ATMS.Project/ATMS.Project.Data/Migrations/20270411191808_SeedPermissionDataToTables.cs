using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermissionDataToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    {  1, "ProjectView",       },
                    {  2, "ProjectEdit",       },
                    {  3, "ProjectDelete",     },
                    {  4, "TicketView",        },
                    {  5, "TicketEdit",        },
                    {  6, "TicketDelete",      },
                    {  7, "TaskView",          },
                    {  8, "TaskEdit",          },
                    {  9, "TaskDelete",        },
                    { 10, "CommentView",       },
                    { 11, "CommentEdit",       },
                    { 12, "CommentDelete",     },
                    { 13, "NotificationView",  },
                    { 14, "NotificationEdit",  },
                    { 15, "NotificationDelete",},
                    { 16, "GroupView",         },
                    { 17, "GroupEdit",         },
                    { 18, "GroupDelete",       },
                    { 19, "DictionaryView",    },
                    { 20, "DictionaryEdit",    },
                    { 21, "DictionaryDelete",  },
                    { 22, "OrganizationView",  },
                    { 23, "OrganizationEdit",  },
                    { 24, "OrganizationDelete",},
                    { 25, "UserView",          },
                    { 26, "UserEdit",          },
                    { 27, "UserDelete",        },
                });

            migrationBuilder.InsertData(
                table: "PermissionTranslations",
                columns: ["PermissionId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    {  1, "en", "Project view"              },
                    {  1, "ru", "Просмотр проектов"         },
                    {  1, "az", "Layihəyə baxış"            },
                    
                    {  2, "en", "Project edit"              },
                    {  2, "ru", "Редактирование проектов"   },
                    {  2, "az", "Layihəni redaktə et"       },
                    
                    {  3, "en", "Project delete"            },
                    {  3, "ru", "Удаление проектов"         },
                    {  3, "az", "Layihəni sil"              },
                    
                    {  4, "en", "Ticket view"               },
                    {  4, "ru", "Просмотр тикетов"          },
                    {  4, "az", "Tiketi baxış"              },
                    
                    {  5, "en", "Ticket edit"               },
                    {  5, "ru", "Редактирование тикетов"    },
                    {  5, "az", "Tiketi redaktə et"         },
                    
                    {  6, "en", "Ticket delete"             },
                    {  6, "ru", "Удаление тикетов"          },
                    {  6, "az", "Tiketi sil"                },
                    
                    {  7, "en", "Task view"                 },
                    {  7, "ru", "Просмотр задач"            },
                    {  7, "az", "Tapşırığa baxış"           },
                    
                    {  8, "en", "Task edit"                 },
                    {  8, "ru", "Редактирование задач"      },
                    {  8, "az", "Tapşırığı redaktə et"      },
                    
                    {  9, "en", "Task delete"               },
                    {  9, "ru", "Удаление задач"            },
                    {  9, "az", "Tapşırığı sil"             },
                    
                    { 10, "en", "Comment view"              },
                    { 10, "ru", "Просмотр комментариев"     },
                    { 10, "az", "Şərhə baxış"               },
                    
                    { 11, "en", "Comment edit"              },
                    { 11, "ru", "Редактирование комментариев"},
                    { 11, "az", "Şərhi redaktə et"          },
                    
                    { 12, "en", "Comment delete"            },
                    { 12, "ru", "Удаление комментариев"     },
                    { 12, "az", "Şərhi sil"                 },
                    
                    { 13, "en", "Notification view"         },
                    { 13, "ru", "Просмотр уведомлений"      },
                    { 13, "az", "Bildirişə baxış"            },
                    
                    { 14, "en", "Notification edit"         },
                    { 14, "ru", "Редактирование уведомлений"},
                    { 14, "az", "Bildirişi redaktə et"       },
                    
                    { 15, "en", "Notification delete"       },
                    { 15, "ru", "Удаление уведомлений"      },
                    { 15, "az", "Bildirişi sil"              },
                    
                    { 16, "en", "Group view"                },
                    { 16, "ru", "Просмотр групп"            },
                    { 16, "az", "Qrupa baxış"               },
                    
                    { 17, "en", "Group edit"                },
                    { 17, "ru", "Редактирование групп"      },
                    { 17, "az", "Qrupu redaktə et"          },
                    
                    { 18, "en", "Group delete"              },
                    { 18, "ru", "Удаление групп"            },
                    { 18, "az", "Qrupu sil"                 },
                    
                    { 19, "en", "Dictionary view"           },
                    { 19, "ru", "Просмотр справочников"     },
                    { 19, "az", "Lüğətə baxış"              },
                    
                    { 20, "en", "Dictionary edit"           },
                    { 20, "ru", "Редактирование справочников"},
                    { 20, "az", "Lüğəti redaktə et"         },
                    
                    { 21, "en", "Dictionary delete"         },
                    { 21, "ru", "Удаление справочников"     },
                    { 21, "az", "Lüğəti sil"                },
                    
                    { 22, "en", "Organization view"         },
                    { 22, "ru", "Просмотр организаций"      },
                    { 22, "az", "Təşkilata baxış"           },
                    
                    { 23, "en", "Organization edit"         },
                    { 23, "ru", "Редактирование организаций"},
                    { 23, "az", "Təşkilatı redaktə et"      },
                    
                    { 24, "en", "Organization delete"       },
                    { 24, "ru", "Удаление организаций"      },
                    { 24, "az", "Təşkilatı sil"             },
                    
                    { 25, "en", "User view"                 },
                    { 25, "ru", "Просмотр пользователей"    },
                    { 25, "az", "İstifadəçiyə baxış"        },
                    
                    { 26, "en", "User edit"                 },
                    { 26, "ru", "Редактирование польз."     },
                    { 26, "az", "İstifadəçini redaktə"      },
                    
                    { 27, "en", "User delete"               },
                    { 27, "ru", "Удаление пользователей"    },
                    { 27, "az", "İstifadəçini sil"          },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (var id = 1; id <= 27; id++)
                migrationBuilder.DeleteData(table: "Permissions", keyColumn: "Id", keyValue: id);
        }
    }
}
