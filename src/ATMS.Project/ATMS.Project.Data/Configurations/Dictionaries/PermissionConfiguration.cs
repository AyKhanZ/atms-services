using ATMS.Data.Enums;
using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();


        builder.HasMany(p => p.Translations)
            .WithOne(t => t.Permission)
            .HasForeignKey(t => t.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasData(
            // Project
            new { Id = (int)ProjectPermissionEnum.ProjectView, Code = "ProjectView" },
            new { Id = (int)ProjectPermissionEnum.ProjectEdit, Code = "ProjectEdit" },
            new { Id = (int)ProjectPermissionEnum.ProjectDelete, Code = "ProjectDelete" },
            // Ticket
            new { Id = (int)ProjectPermissionEnum.TicketView, Code = "TicketView" },
            new { Id = (int)ProjectPermissionEnum.TicketEdit, Code = "TicketEdit" },
            new { Id = (int)ProjectPermissionEnum.TicketDelete, Code = "TicketDelete" },
            // Task
            new { Id = (int)ProjectPermissionEnum.TaskView, Code = "TaskView" },
            new { Id = (int)ProjectPermissionEnum.TaskEdit, Code = "TaskEdit" },
            new { Id = (int)ProjectPermissionEnum.TaskDelete, Code = "TaskDelete" },
            // Comment
            new { Id = (int)ProjectPermissionEnum.CommentView, Code = "CommentView" },
            new { Id = (int)ProjectPermissionEnum.CommentEdit, Code = "CommentEdit" },
            new { Id = (int)ProjectPermissionEnum.CommentDelete, Code = "CommentDelete" },
            // Notification
            new { Id = (int)ProjectPermissionEnum.NotificationView, Code = "NotificationView" },
            new { Id = (int)ProjectPermissionEnum.NotificationEdit, Code = "NotificationEdit" },
            new { Id = (int)ProjectPermissionEnum.NotificationDelete, Code = "NotificationDelete" },
            // Group
            new { Id = (int)ProjectPermissionEnum.GroupView, Code = "GroupView" },
            new { Id = (int)ProjectPermissionEnum.GroupEdit, Code = "GroupEdit" },
            new { Id = (int)ProjectPermissionEnum.GroupDelete, Code = "GroupDelete" },
            // Dictionary
            new { Id = (int)ProjectPermissionEnum.DictionaryView, Code = "DictionaryView" },
            new { Id = (int)ProjectPermissionEnum.DictionaryEdit, Code = "DictionaryEdit" },
            new { Id = (int)ProjectPermissionEnum.DictionaryDelete, Code = "DictionaryDelete" },
            // Organization
            new { Id = (int)ProjectPermissionEnum.OrganizationView, Code = "OrganizationView" },
            new { Id = (int)ProjectPermissionEnum.OrganizationEdit, Code = "OrganizationEdit" },
            new { Id = (int)ProjectPermissionEnum.OrganizationDelete, Code = "OrganizationDelete" },
            // User
            new { Id = (int)ProjectPermissionEnum.UserView, Code = "UserView" },
            new { Id = (int)ProjectPermissionEnum.UserEdit, Code = "UserEdit" },
            new { Id = (int)ProjectPermissionEnum.UserDelete, Code = "UserDelete" },
            new { Id = (int)ProjectPermissionEnum.UserInvite, Code = "UserInvite" }
        );
    }
}

public class PermissionTranslationConfiguration : IEntityTypeConfiguration<PermissionTranslation>
{
    public void Configure(EntityTypeBuilder<PermissionTranslation> builder)
    {
        builder.HasIndex(t => new { t.PermissionId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.HasData(
            // Project
            new { Id = 1, PermissionId = (int)ProjectPermissionEnum.ProjectView, Language = "en", Name = "Project view" },
            new { Id = 2, PermissionId = (int)ProjectPermissionEnum.ProjectView, Language = "ru", Name = "Просмотр проектов" },
            new { Id = 3, PermissionId = (int)ProjectPermissionEnum.ProjectView, Language = "az", Name = "Layihəyə baxış" },
            new { Id = 4, PermissionId = (int)ProjectPermissionEnum.ProjectEdit, Language = "en", Name = "Project edit" },
            new { Id = 5, PermissionId = (int)ProjectPermissionEnum.ProjectEdit, Language = "ru", Name = "Редактирование проектов" },
            new { Id = 6, PermissionId = (int)ProjectPermissionEnum.ProjectEdit, Language = "az", Name = "Layihəni redaktə et" },
            new { Id = 7, PermissionId = (int)ProjectPermissionEnum.ProjectDelete, Language = "en", Name = "Project delete" },
            new { Id = 8, PermissionId = (int)ProjectPermissionEnum.ProjectDelete, Language = "ru", Name = "Удаление проектов" },
            new { Id = 9, PermissionId = (int)ProjectPermissionEnum.ProjectDelete, Language = "az", Name = "Layihəni sil" },
            // Ticket
            new { Id = 10, PermissionId = (int)ProjectPermissionEnum.TicketView, Language = "en", Name = "Ticket view" },
            new { Id = 11, PermissionId = (int)ProjectPermissionEnum.TicketView, Language = "ru", Name = "Просмотр тикетов" },
            new { Id = 12, PermissionId = (int)ProjectPermissionEnum.TicketView, Language = "az", Name = "Tiketi baxış" },
            new { Id = 13, PermissionId = (int)ProjectPermissionEnum.TicketEdit, Language = "en", Name = "Ticket edit" },
            new { Id = 14, PermissionId = (int)ProjectPermissionEnum.TicketEdit, Language = "ru", Name = "Редактирование тикетов" },
            new { Id = 15, PermissionId = (int)ProjectPermissionEnum.TicketEdit, Language = "az", Name = "Tiketi redaktə et" },
            new { Id = 16, PermissionId = (int)ProjectPermissionEnum.TicketDelete, Language = "en", Name = "Ticket delete" },
            new { Id = 17, PermissionId = (int)ProjectPermissionEnum.TicketDelete, Language = "ru", Name = "Удаление тикетов" },
            new { Id = 18, PermissionId = (int)ProjectPermissionEnum.TicketDelete, Language = "az", Name = "Tiketi sil" },
            // Task
            new { Id = 19, PermissionId = (int)ProjectPermissionEnum.TaskView, Language = "en", Name = "Task view" },
            new { Id = 20, PermissionId = (int)ProjectPermissionEnum.TaskView, Language = "ru", Name = "Просмотр задач" },
            new { Id = 21, PermissionId = (int)ProjectPermissionEnum.TaskView, Language = "az", Name = "Tapşırığa baxış" },
            new { Id = 22, PermissionId = (int)ProjectPermissionEnum.TaskEdit, Language = "en", Name = "Task edit" },
            new { Id = 23, PermissionId = (int)ProjectPermissionEnum.TaskEdit, Language = "ru", Name = "Редактирование задач" },
            new { Id = 24, PermissionId = (int)ProjectPermissionEnum.TaskEdit, Language = "az", Name = "Tapşırığı redaktə et" },
            new { Id = 25, PermissionId = (int)ProjectPermissionEnum.TaskDelete, Language = "en", Name = "Task delete" },
            new { Id = 26, PermissionId = (int)ProjectPermissionEnum.TaskDelete, Language = "ru", Name = "Удаление задач" },
            new { Id = 27, PermissionId = (int)ProjectPermissionEnum.TaskDelete, Language = "az", Name = "Tapşırığı sil" },
            // Comment
            new { Id = 28, PermissionId = (int)ProjectPermissionEnum.CommentView, Language = "en", Name = "Comment view" },
            new { Id = 29, PermissionId = (int)ProjectPermissionEnum.CommentView, Language = "ru", Name = "Просмотр комментариев" },
            new { Id = 30, PermissionId = (int)ProjectPermissionEnum.CommentView, Language = "az", Name = "Şərhə baxış" },
            new { Id = 31, PermissionId = (int)ProjectPermissionEnum.CommentEdit, Language = "en", Name = "Comment edit" },
            new { Id = 32, PermissionId = (int)ProjectPermissionEnum.CommentEdit, Language = "ru", Name = "Редактирование комментариев" },
            new { Id = 33, PermissionId = (int)ProjectPermissionEnum.CommentEdit, Language = "az", Name = "Şərhi redaktə et" },
            new { Id = 34, PermissionId = (int)ProjectPermissionEnum.CommentDelete, Language = "en", Name = "Comment delete" },
            new { Id = 35, PermissionId = (int)ProjectPermissionEnum.CommentDelete, Language = "ru", Name = "Удаление комментариев" },
            new { Id = 36, PermissionId = (int)ProjectPermissionEnum.CommentDelete, Language = "az", Name = "Şərhi sil" },
            // Notification
            new { Id = 37, PermissionId = (int)ProjectPermissionEnum.NotificationView, Language = "en", Name = "Notification view" },
            new { Id = 38, PermissionId = (int)ProjectPermissionEnum.NotificationView, Language = "ru", Name = "Просмотр уведомлений" },
            new { Id = 39, PermissionId = (int)ProjectPermissionEnum.NotificationView, Language = "az", Name = "Bildirişə baxış" },
            new { Id = 40, PermissionId = (int)ProjectPermissionEnum.NotificationEdit, Language = "en", Name = "Notification edit" },
            new { Id = 41, PermissionId = (int)ProjectPermissionEnum.NotificationEdit, Language = "ru", Name = "Редактирование уведомлений" },
            new { Id = 42, PermissionId = (int)ProjectPermissionEnum.NotificationEdit, Language = "az", Name = "Bildirişi redaktə et" },
            new { Id = 43, PermissionId = (int)ProjectPermissionEnum.NotificationDelete, Language = "en", Name = "Notification delete" },
            new { Id = 44, PermissionId = (int)ProjectPermissionEnum.NotificationDelete, Language = "ru", Name = "Удаление уведомлений" },
            new { Id = 45, PermissionId = (int)ProjectPermissionEnum.NotificationDelete, Language = "az", Name = "Bildirişi sil" },
            // Group
            new { Id = 46, PermissionId = (int)ProjectPermissionEnum.GroupView, Language = "en", Name = "Group view" },
            new { Id = 47, PermissionId = (int)ProjectPermissionEnum.GroupView, Language = "ru", Name = "Просмотр групп" },
            new { Id = 48, PermissionId = (int)ProjectPermissionEnum.GroupView, Language = "az", Name = "Qrupa baxış" },
            new { Id = 49, PermissionId = (int)ProjectPermissionEnum.GroupEdit, Language = "en", Name = "Group edit" },
            new { Id = 50, PermissionId = (int)ProjectPermissionEnum.GroupEdit, Language = "ru", Name = "Редактирование групп" },
            new { Id = 51, PermissionId = (int)ProjectPermissionEnum.GroupEdit, Language = "az", Name = "Qrupu redaktə et" },
            new { Id = 52, PermissionId = (int)ProjectPermissionEnum.GroupDelete, Language = "en", Name = "Group delete" },
            new { Id = 53, PermissionId = (int)ProjectPermissionEnum.GroupDelete, Language = "ru", Name = "Удаление групп" },
            new { Id = 54, PermissionId = (int)ProjectPermissionEnum.GroupDelete, Language = "az", Name = "Qrupu sil" },
            // Dictionary
            new { Id = 55, PermissionId = (int)ProjectPermissionEnum.DictionaryView, Language = "en", Name = "Dictionary view" },
            new { Id = 56, PermissionId = (int)ProjectPermissionEnum.DictionaryView, Language = "ru", Name = "Просмотр справочников" },
            new { Id = 57, PermissionId = (int)ProjectPermissionEnum.DictionaryView, Language = "az", Name = "Lüğətə baxış" },
            new { Id = 58, PermissionId = (int)ProjectPermissionEnum.DictionaryEdit, Language = "en", Name = "Dictionary edit" },
            new { Id = 59, PermissionId = (int)ProjectPermissionEnum.DictionaryEdit, Language = "ru", Name = "Редактирование справочников" },
            new { Id = 60, PermissionId = (int)ProjectPermissionEnum.DictionaryEdit, Language = "az", Name = "Lüğəti redaktə et" },
            new { Id = 61, PermissionId = (int)ProjectPermissionEnum.DictionaryDelete, Language = "en", Name = "Dictionary delete" },
            new { Id = 62, PermissionId = (int)ProjectPermissionEnum.DictionaryDelete, Language = "ru", Name = "Удаление справочников" },
            new { Id = 63, PermissionId = (int)ProjectPermissionEnum.DictionaryDelete, Language = "az", Name = "Lüğəti sil" },
            // Organization
            new { Id = 64, PermissionId = (int)ProjectPermissionEnum.OrganizationView, Language = "en", Name = "Organization view" },
            new { Id = 65, PermissionId = (int)ProjectPermissionEnum.OrganizationView, Language = "ru", Name = "Просмотр организаций" },
            new { Id = 66, PermissionId = (int)ProjectPermissionEnum.OrganizationView, Language = "az", Name = "Təşkilata baxış" },
            new { Id = 67, PermissionId = (int)ProjectPermissionEnum.OrganizationEdit, Language = "en", Name = "Organization edit" },
            new { Id = 68, PermissionId = (int)ProjectPermissionEnum.OrganizationEdit, Language = "ru", Name = "Редактирование организаций" },
            new { Id = 69, PermissionId = (int)ProjectPermissionEnum.OrganizationEdit, Language = "az", Name = "Təşkilatı redaktə et" },
            new { Id = 70, PermissionId = (int)ProjectPermissionEnum.OrganizationDelete, Language = "en", Name = "Organization delete" },
            new { Id = 71, PermissionId = (int)ProjectPermissionEnum.OrganizationDelete, Language = "ru", Name = "Удаление организаций" },
            new { Id = 72, PermissionId = (int)ProjectPermissionEnum.OrganizationDelete, Language = "az", Name = "Təşkilatı sil" },
            // User
            new { Id = 73, PermissionId = (int)ProjectPermissionEnum.UserView, Language = "en", Name = "User view" },
            new { Id = 74, PermissionId = (int)ProjectPermissionEnum.UserView, Language = "ru", Name = "Просмотр пользователей" },
            new { Id = 75, PermissionId = (int)ProjectPermissionEnum.UserView, Language = "az", Name = "İstifadəçiyə baxış" },
            new { Id = 76, PermissionId = (int)ProjectPermissionEnum.UserEdit, Language = "en", Name = "User edit" },
            new { Id = 77, PermissionId = (int)ProjectPermissionEnum.UserEdit, Language = "ru", Name = "Редактирование польз." },
            new { Id = 78, PermissionId = (int)ProjectPermissionEnum.UserEdit, Language = "az", Name = "İstifadəçini redaktə" },
            new { Id = 79, PermissionId = (int)ProjectPermissionEnum.UserDelete, Language = "en", Name = "User delete" },
            new { Id = 80, PermissionId = (int)ProjectPermissionEnum.UserDelete, Language = "ru", Name = "Удаление пользователей" },
            new { Id = 81, PermissionId = (int)ProjectPermissionEnum.UserDelete, Language = "az", Name = "İstifadəçини sil" }
        );
    }
}