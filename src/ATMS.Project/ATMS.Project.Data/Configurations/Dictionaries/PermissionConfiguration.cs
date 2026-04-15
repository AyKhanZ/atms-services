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
            new { Id = 1, Code = "ProjectView" },
            new { Id = 2, Code = "ProjectEdit" },
            new { Id = 3, Code = "ProjectDelete" },
            // Ticket
            new { Id = 4, Code = "TicketView" },
            new { Id = 5, Code = "TicketEdit" },
            new { Id = 6, Code = "TicketDelete" },
            // Task
            new { Id = 7, Code = "TaskView" },
            new { Id = 8, Code = "TaskEdit" },
            new { Id = 9, Code = "TaskDelete" },
            // Comment
            new { Id = 10, Code = "CommentView" },
            new { Id = 11, Code = "CommentEdit" },
            new { Id = 12, Code = "CommentDelete" },
            // Notification
            new { Id = 13, Code = "NotificationView" },
            new { Id = 14, Code = "NotificationEdit" },
            new { Id = 15, Code = "NotificationDelete" },
            // Group
            new { Id = 16, Code = "GroupView" },
            new { Id = 17, Code = "GroupEdit" },
            new { Id = 18, Code = "GroupDelete" },
            // Dictionary
            new { Id = 19, Code = "DictionaryView" },
            new { Id = 20, Code = "DictionaryEdit" },
            new { Id = 21, Code = "DictionaryDelete" },
            // Organization
            new { Id = 22, Code = "OrganizationView" },
            new { Id = 23, Code = "OrganizationEdit" },
            new { Id = 24, Code = "OrganizationDelete" },
            // User
            new { Id = 25, Code = "UserView" },
            new { Id = 26, Code = "UserEdit" },
            new { Id = 27, Code = "UserDelete" }
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
            new { Id = 1, PermissionId = 1, Language = "en", Name = "Project view" },
            new { Id = 2, PermissionId = 1, Language = "ru", Name = "Просмотр проектов" },
            new { Id = 3, PermissionId = 1, Language = "az", Name = "Layihəyə baxış" },
            new { Id = 4, PermissionId = 2, Language = "en", Name = "Project edit" },
            new { Id = 5, PermissionId = 2, Language = "ru", Name = "Редактирование проектов" },
            new { Id = 6, PermissionId = 2, Language = "az", Name = "Layihəni redaktə et" },
            new { Id = 7, PermissionId = 3, Language = "en", Name = "Project delete" },
            new { Id = 8, PermissionId = 3, Language = "ru", Name = "Удаление проектов" },
            new { Id = 9, PermissionId = 3, Language = "az", Name = "Layihəni sil" },
            // Ticket
            new { Id = 10, PermissionId = 4, Language = "en", Name = "Ticket view" },
            new { Id = 11, PermissionId = 4, Language = "ru", Name = "Просмотр тикетов" },
            new { Id = 12, PermissionId = 4, Language = "az", Name = "Tiketi baxış" },
            new { Id = 13, PermissionId = 5, Language = "en", Name = "Ticket edit" },
            new { Id = 14, PermissionId = 5, Language = "ru", Name = "Редактирование тикетов" },
            new { Id = 15, PermissionId = 5, Language = "az", Name = "Tiketi redaktə et" },
            new { Id = 16, PermissionId = 6, Language = "en", Name = "Ticket delete" },
            new { Id = 17, PermissionId = 6, Language = "ru", Name = "Удаление тикетов" },
            new { Id = 18, PermissionId = 6, Language = "az", Name = "Tiketi sil" },
            // Task
            new { Id = 19, PermissionId = 7, Language = "en", Name = "Task view" },
            new { Id = 20, PermissionId = 7, Language = "ru", Name = "Просмотр задач" },
            new { Id = 21, PermissionId = 7, Language = "az", Name = "Tapşırığa baxış" },
            new { Id = 22, PermissionId = 8, Language = "en", Name = "Task edit" },
            new { Id = 23, PermissionId = 8, Language = "ru", Name = "Редактирование задач" },
            new { Id = 24, PermissionId = 8, Language = "az", Name = "Tapşırığı redaktə et" },
            new { Id = 25, PermissionId = 9, Language = "en", Name = "Task delete" },
            new { Id = 26, PermissionId = 9, Language = "ru", Name = "Удаление задач" },
            new { Id = 27, PermissionId = 9, Language = "az", Name = "Tapşırığı sil" },
            // Comment
            new { Id = 28, PermissionId = 10, Language = "en", Name = "Comment view" },
            new { Id = 29, PermissionId = 10, Language = "ru", Name = "Просмотр комментариев" },
            new { Id = 30, PermissionId = 10, Language = "az", Name = "Şərhə baxış" },
            new { Id = 31, PermissionId = 11, Language = "en", Name = "Comment edit" },
            new { Id = 32, PermissionId = 11, Language = "ru", Name = "Редактирование комментариев" },
            new { Id = 33, PermissionId = 11, Language = "az", Name = "Şərhi redaktə et" },
            new { Id = 34, PermissionId = 12, Language = "en", Name = "Comment delete" },
            new { Id = 35, PermissionId = 12, Language = "ru", Name = "Удаление комментариев" },
            new { Id = 36, PermissionId = 12, Language = "az", Name = "Şərhi sil" },
            // Notification
            new { Id = 37, PermissionId = 13, Language = "en", Name = "Notification view" },
            new { Id = 38, PermissionId = 13, Language = "ru", Name = "Просмотр уведомлений" },
            new { Id = 39, PermissionId = 13, Language = "az", Name = "Bildirişə baxış" },
            new { Id = 40, PermissionId = 14, Language = "en", Name = "Notification edit" },
            new { Id = 41, PermissionId = 14, Language = "ru", Name = "Редактирование уведомлений" },
            new { Id = 42, PermissionId = 14, Language = "az", Name = "Bildirişi redaktə et" },
            new { Id = 43, PermissionId = 15, Language = "en", Name = "Notification delete" },
            new { Id = 44, PermissionId = 15, Language = "ru", Name = "Удаление уведомлений" },
            new { Id = 45, PermissionId = 15, Language = "az", Name = "Bildirişi sil" },
            // Group
            new { Id = 46, PermissionId = 16, Language = "en", Name = "Group view" },
            new { Id = 47, PermissionId = 16, Language = "ru", Name = "Просмотр групп" },
            new { Id = 48, PermissionId = 16, Language = "az", Name = "Qrupa baxış" },
            new { Id = 49, PermissionId = 17, Language = "en", Name = "Group edit" },
            new { Id = 50, PermissionId = 17, Language = "ru", Name = "Редактирование групп" },
            new { Id = 51, PermissionId = 17, Language = "az", Name = "Qrupu redaktə et" },
            new { Id = 52, PermissionId = 18, Language = "en", Name = "Group delete" },
            new { Id = 53, PermissionId = 18, Language = "ru", Name = "Удаление групп" },
            new { Id = 54, PermissionId = 18, Language = "az", Name = "Qrupu sil" },
            // Dictionary
            new { Id = 55, PermissionId = 19, Language = "en", Name = "Dictionary view" },
            new { Id = 56, PermissionId = 19, Language = "ru", Name = "Просмотр справочников" },
            new { Id = 57, PermissionId = 19, Language = "az", Name = "Lüğətə baxış" },
            new { Id = 58, PermissionId = 20, Language = "en", Name = "Dictionary edit" },
            new { Id = 59, PermissionId = 20, Language = "ru", Name = "Редактирование справочников" },
            new { Id = 60, PermissionId = 20, Language = "az", Name = "Lüğəti redaktə et" },
            new { Id = 61, PermissionId = 21, Language = "en", Name = "Dictionary delete" },
            new { Id = 62, PermissionId = 21, Language = "ru", Name = "Удаление справочников" },
            new { Id = 63, PermissionId = 21, Language = "az", Name = "Lüğəti sil" },
            // Organization
            new { Id = 64, PermissionId = 22, Language = "en", Name = "Organization view" },
            new { Id = 65, PermissionId = 22, Language = "ru", Name = "Просмотр организаций" },
            new { Id = 66, PermissionId = 22, Language = "az", Name = "Təşkilata baxış" },
            new { Id = 67, PermissionId = 23, Language = "en", Name = "Organization edit" },
            new { Id = 68, PermissionId = 23, Language = "ru", Name = "Редактирование организаций" },
            new { Id = 69, PermissionId = 23, Language = "az", Name = "Təşkilatı redaktə et" },
            new { Id = 70, PermissionId = 24, Language = "en", Name = "Organization delete" },
            new { Id = 71, PermissionId = 24, Language = "ru", Name = "Удаление организаций" },
            new { Id = 72, PermissionId = 24, Language = "az", Name = "Təşkilatı sil" },
            // User
            new { Id = 73, PermissionId = 25, Language = "en", Name = "User view" },
            new { Id = 74, PermissionId = 25, Language = "ru", Name = "Просмотр пользователей" },
            new { Id = 75, PermissionId = 25, Language = "az", Name = "İstifadəçiyə baxış" },
            new { Id = 76, PermissionId = 26, Language = "en", Name = "User edit" },
            new { Id = 77, PermissionId = 26, Language = "ru", Name = "Редактирование польз." },
            new { Id = 78, PermissionId = 26, Language = "az", Name = "İstifadəçini redaktə" },
            new { Id = 79, PermissionId = 27, Language = "en", Name = "User delete" },
            new { Id = 80, PermissionId = 27, Language = "ru", Name = "Удаление пользователей" },
            new { Id = 81, PermissionId = 27, Language = "az", Name = "İstifadəçини sil" }
        );
    }
}