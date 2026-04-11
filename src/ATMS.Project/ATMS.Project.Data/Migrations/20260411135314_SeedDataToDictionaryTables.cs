using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataToDictionaryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "WorkTicketTypes",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "Bug"     },
                    { 2, "Feature" },
                    { 3, "Task"    }
                });

            migrationBuilder.InsertData(
                table: "WorkTicketTypeTranslations",
                columns: ["WorkTicketTypeId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Bug"           },
                    { 1, "ru", "Ошибка"        },
                    { 1, "az", "Xəta"          },
                    
                    { 2, "en", "Feature"        },
                    { 2, "ru", "Новая функция"  },
                    { 2, "az", "Təzə Funksiya"  },
                    
                    { 3, "en", "Task"           },
                    { 3, "ru", "Задача"         },
                    { 3, "az", "Tapşırıq"       }
                });
            
            
            
            migrationBuilder.InsertData(
                table: "WorkTicketStatuses",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "New"     },
                    { 2, "InProgress" },
                    { 3, "InReview" },
                    { 4, "Testing" },
                    { 5, "Closed" },
                    { 6, "Rejected"    }
                });

            migrationBuilder.InsertData(
                table: "WorkTicketStatusTranslations",
                columns: ["WorkTicketStatusId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "New"          },
                    { 1, "ru", "Новый"        },
                    { 1, "az", "Yeni"         },
                    
                    { 2, "en", "In Progress"  },
                    { 2, "ru", "В работе"     },
                    { 2, "az", "İşdə"         },
                    
                    { 3, "en", "In Review"    },
                    { 3, "ru", "На проверке"  },
                    { 3, "az", "Yoxlamada"    },
                    
                    { 4, "en", "Testing"           },
                    { 4, "ru", "Тестирование"      },
                    { 4, "az", "Test mərhələsində" },
                    
                    { 5, "en", "Closed"        },
                    { 5, "ru", "Закрыт"        },
                    { 5, "az", "Bağlandı"      },
                    
                    { 6, "en", "Rejected"      },
                    { 6, "ru", "Отклонён"      },
                    { 6, "az", "Rədd edildi"   }
                });
            
            
            
            migrationBuilder.InsertData(
                table: "WorkTaskStatuses",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "New"     },
                    { 2, "InProgress" },
                    { 3, "Done" }
                });

            migrationBuilder.InsertData(
                table: "WorkTaskStatusTranslations",
                columns: ["WorkTaskStatusId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "New"          },
                    { 1, "ru", "Новый"        },
                    { 1, "az", "Yeni"         },
                    
                    { 2, "en", "In Progress"  },
                    { 2, "ru", "В работе"     },
                    { 2, "az", "İşdə"         },
                    
                    { 3, "en", "Done"    },
                    { 3, "ru", "Выполнено"  },
                    { 3, "az", "Hazır"    }
                });
            
            
            
            migrationBuilder.InsertData(
                table: "WorkItemPriorities",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "Low"     },
                    { 2, "Medium" },
                    { 3, "High" }
                });

            migrationBuilder.InsertData(
                table: "WorkItemPriorityTranslations",
                columns: ["WorkItemPriorityId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Low" },
                    { 1, "ru", "Низкий" },
                    { 1, "az", "Aşağı" },

                    { 2, "en", "Medium" },
                    { 2, "ru", "Средний" },
                    { 2, "az", "Orta" },

                    { 3, "en", "High" },
                    { 3, "ru", "Высокий" },
                    { 3, "az", "Yüksək" }
                });
            
            
            
            migrationBuilder.InsertData(
                table: "WorkGroupStatuses",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "Planned"     },
                    { 2, "Active" },
                    { 3, "Done" }
                });

            migrationBuilder.InsertData(
                table: "WorkGroupStatusTranslations",
                columns: ["WorkGroupStatusId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Planned" },
                    { 1, "ru", "Запланировано" },
                    { 1, "az", "Planlaşdırılıb" },

                    { 2, "en", "Active" },
                    { 2, "ru", "Активный" },
                    { 2, "az", "Aktiv" },

                    { 3, "en", "Done" },
                    { 3, "ru", "Завершено" },
                    { 3, "az", "Bitdi" }
                });
            
            
            
            migrationBuilder.InsertData(
                table: "ProjectTypes",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "Standard"     },
                    { 2, "Optimal" },
                    { 3, "Premium" }
                });

            migrationBuilder.InsertData(
                table: "ProjectTypeTranslations",
                columns: ["ProjectTypeId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Standard" },
                    { 1, "ru", "Стандартный" },
                    { 1, "az", "Standart" },

                    { 2, "en", "Optimal" },
                    { 2, "ru", "Оптимальный" },
                    { 2, "az", "Optimal" },

                    { 3, "en", "Premium" },
                    { 3, "ru", "Премиум" },
                    { 3, "az", "Premium" }
                });
            
            
            
            migrationBuilder.InsertData(
                table: "ProjectStatuses",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "Draft"     },
                    { 2, "Active" },
                    { 3, "OnReview" },
                    { 4, "Closed" }
                });

            migrationBuilder.InsertData(
                table: "ProjectStatusTranslations",
                columns: ["ProjectStatusId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Draft" },
                    { 1, "ru", "Черновик" },
                    { 1, "az", "Qaralama" },

                    { 2, "en", "Active" },
                    { 2, "ru", "Активный" },
                    { 2, "az", "Aktiv" },

                    { 3, "en", "In Review" },
                    { 3, "ru", "На проверке" },
                    { 3, "az", "Yoxlamada" },

                    { 4, "en", "Closed" },
                    { 4, "ru", "Закрыт" },
                    { 4, "az", "Bağlanıb" }
                });
            
            
            
            migrationBuilder.InsertData(
                table: "ProjectKinds",
                columns: ["Id", "Code"],
                columnTypes: ["integer", "character varying(50)"],
                values: new object[,]
                {
                    { 1, "Support"     },
                    { 2, "External" },
                    { 3, "Internal" },
                    { 4, "OneTime" }
                });

            migrationBuilder.InsertData(
                table: "ProjectKindTranslations",
                columns: ["ProjectKindId", "Language", "Name"],
                columnTypes: ["integer", "character varying(5)", "character varying(100)"],
                values: new object[,]
                {
                    { 1, "en", "Support" },
                    { 1, "ru", "Поддержка" },
                    { 1, "az", "Dəstək" },

                    { 2, "en", "External" },
                    { 2, "ru", "Внешний" },
                    { 2, "az", "Xarici" },

                    { 3, "en", "Internal" },
                    { 3, "ru", "Внутренний" },
                    { 3, "az", "Daxili" },

                    { 4, "en", "One Time" },
                    { 4, "ru", "Разовый" },
                    { 4, "az", "Birdəfəlik" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "WorkTicketTypes", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "WorkTicketTypes", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "WorkTicketTypes", keyColumn: "Id", keyValue: 3);
            
            
            migrationBuilder.DeleteData(table: "WorkTicketStatuses", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "WorkTicketStatuses", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "WorkTicketStatuses", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "WorkTicketStatuses", keyColumn: "Id", keyValue: 4);
            migrationBuilder.DeleteData(table: "WorkTicketStatuses", keyColumn: "Id", keyValue: 5);
            migrationBuilder.DeleteData(table: "WorkTicketStatuses", keyColumn: "Id", keyValue: 6);
            
            
            migrationBuilder.DeleteData(table: "WorkTaskStatuses", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "WorkTaskStatuses", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "WorkTaskStatuses", keyColumn: "Id", keyValue: 3);
            
            
            migrationBuilder.DeleteData(table: "WorkItemPriorities", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "WorkItemPriorities", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "WorkItemPriorities", keyColumn: "Id", keyValue: 3);
            
            
            migrationBuilder.DeleteData(table: "WorkGroupStatuses", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "WorkGroupStatuses", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "WorkGroupStatuses", keyColumn: "Id", keyValue: 3);
            
            
            migrationBuilder.DeleteData(table: "ProjectTypes", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "ProjectTypes", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "ProjectTypes", keyColumn: "Id", keyValue: 3);
            
            
            migrationBuilder.DeleteData(table: "ProjectStatuses", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "ProjectStatuses", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "ProjectStatuses", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "ProjectStatuses", keyColumn: "Id", keyValue: 4);
            
            
            migrationBuilder.DeleteData(table: "ProjectKinds", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "ProjectKinds", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "ProjectKinds", keyColumn: "Id", keyValue: 3);
        }
    }
}
