using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations.Dictionaries;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();


        builder.HasMany(p => p.Translations)
            .WithOne(t => t.Permission)
            .HasForeignKey(t => t.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasData(
            new { Id = 1, Code = "ProjectView" },
            new { Id = 2, Code = "ProjectEdit" },
            new { Id = 3, Code = "ProjectDelete" },
            new { Id = 4, Code = "TicketView" },
            new { Id = 5, Code = "TicketEdit" },
            new { Id = 6, Code = "TicketDelete" },
            new { Id = 7, Code = "TaskView" },
            new { Id = 8, Code = "TaskEdit" },
            new { Id = 9, Code = "TaskDelete" },
            new { Id = 10, Code = "CommentView" },
            new { Id = 11, Code = "CommentEdit" },
            new { Id = 12, Code = "CommentDelete" },
            new { Id = 13, Code = "NotificationView" },
            new { Id = 14, Code = "NotificationEdit" },
            new { Id = 15, Code = "NotificationDelete" },
            new { Id = 16, Code = "GroupView" },
            new { Id = 17, Code = "GroupEdit" },
            new { Id = 18, Code = "GroupDelete" },
            new { Id = 19, Code = "DictionaryView" },
            new { Id = 20, Code = "DictionaryEdit" },
            new { Id = 21, Code = "DictionaryDelete" },
            new { Id = 22, Code = "OrganizationView" },
            new { Id = 23, Code = "OrganizationEdit" },
            new { Id = 24, Code = "OrganizationDelete" },
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
        builder.HasIndex(t => new { t.PermissionId, t.Language }).IsUnique();

        builder.Property(t => t.Language).HasMaxLength(5).IsRequired();

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();


        builder.HasData(
            new { PermissionId = 1, Language = "en", Name = "Project view" },
            new { PermissionId = 1, Language = "ru", Name = "Просмотр проектов" },
            new { PermissionId = 1, Language = "az", Name = "Layihəyə baxış" },
            new { PermissionId = 2, Language = "en", Name = "Project edit" },
            new { PermissionId = 2, Language = "ru", Name = "Редактирование проектов" },
            new { PermissionId = 2, Language = "az", Name = "Layihəni redaktə et" },
            new { PermissionId = 3, Language = "en", Name = "Project delete" },
            new { PermissionId = 3, Language = "ru", Name = "Удаление проектов" },
            new { PermissionId = 3, Language = "az", Name = "Layihəni sil" },
            new { PermissionId = 4, Language = "en", Name = "Ticket view" },
            new { PermissionId = 4, Language = "ru", Name = "Просмотр тикетов" },
            new { PermissionId = 4, Language = "az", Name = "Tiketi baxış" },
            new { PermissionId = 5, Language = "en", Name = "Ticket edit" },
            new { PermissionId = 5, Language = "ru", Name = "Редактирование тикетов" },
            new { PermissionId = 5, Language = "az", Name = "Tiketi redaktə et" },
            new { PermissionId = 6, Language = "en", Name = "Ticket delete" },
            new { PermissionId = 6, Language = "ru", Name = "Удаление тикетов" },
            new { PermissionId = 6, Language = "az", Name = "Tiketi sil" },
            new { PermissionId = 7, Language = "en", Name = "Task view" },
            new { PermissionId = 7, Language = "ru", Name = "Просмотр задач" },
            new { PermissionId = 7, Language = "az", Name = "Tapşırığa baxış" },
            new { PermissionId = 8, Language = "en", Name = "Task edit" },
            new { PermissionId = 8, Language = "ru", Name = "Редактирование задач" },
            new { PermissionId = 8, Language = "az", Name = "Tapşırığı redaktə et" },
            new { PermissionId = 9, Language = "en", Name = "Task delete" },
            new { PermissionId = 9, Language = "ru", Name = "Удаление задач" },
            new { PermissionId = 9, Language = "az", Name = "Tapşırığı sil" },
            new { PermissionId = 10, Language = "en", Name = "Comment view" },
            new { PermissionId = 10, Language = "ru", Name = "Просмотр комментариев" },
            new { PermissionId = 10, Language = "az", Name = "Şərhə baxış" },
            new { PermissionId = 11, Language = "en", Name = "Comment edit" },
            new { PermissionId = 11, Language = "ru", Name = "Редактирование комментариев" },
            new { PermissionId = 11, Language = "az", Name = "Şərhi redaktə et" },
            new { PermissionId = 12, Language = "en", Name = "Comment delete" },
            new { PermissionId = 12, Language = "ru", Name = "Удаление комментариев" },
            new { PermissionId = 12, Language = "az", Name = "Şərhi sil" },
            new { PermissionId = 13, Language = "en", Name = "Notification view" },
            new { PermissionId = 13, Language = "ru", Name = "Просмотр уведомлений" },
            new { PermissionId = 13, Language = "az", Name = "Bildirişə baxış" },
            new { PermissionId = 14, Language = "en", Name = "Notification edit" },
            new { PermissionId = 14, Language = "ru", Name = "Редактирование уведомлений" },
            new { PermissionId = 14, Language = "az", Name = "Bildirişi redaktə et" },
            new { PermissionId = 15, Language = "en", Name = "Notification delete" },
            new { PermissionId = 15, Language = "ru", Name = "Удаление уведомлений" },
            new { PermissionId = 15, Language = "az", Name = "Bildirişi sil" },
            new { PermissionId = 16, Language = "en", Name = "Group view" },
            new { PermissionId = 16, Language = "ru", Name = "Просмотр групп" },
            new { PermissionId = 16, Language = "az", Name = "Qrupa baxış" },
            new { PermissionId = 17, Language = "en", Name = "Group edit" },
            new { PermissionId = 17, Language = "ru", Name = "Редактирование групп" },
            new { PermissionId = 17, Language = "az", Name = "Qrupu redaktə et" },
            new { PermissionId = 18, Language = "en", Name = "Group delete" },
            new { PermissionId = 18, Language = "ru", Name = "Удаление групп" },
            new { PermissionId = 18, Language = "az", Name = "Qrupu sil" },
            new { PermissionId = 19, Language = "en", Name = "Dictionary view" },
            new { PermissionId = 19, Language = "ru", Name = "Просмотр справочников" },
            new { PermissionId = 19, Language = "az", Name = "Lüğətə baxış" },
            new { PermissionId = 20, Language = "en", Name = "Dictionary edit" },
            new { PermissionId = 20, Language = "ru", Name = "Редактирование справочников" },
            new { PermissionId = 20, Language = "az", Name = "Lüğəti redaktə et" },
            new { PermissionId = 21, Language = "en", Name = "Dictionary delete" },
            new { PermissionId = 21, Language = "ru", Name = "Удаление справочников" },
            new { PermissionId = 21, Language = "az", Name = "Lüğəti sil" },
            new { PermissionId = 22, Language = "en", Name = "Organization view" },
            new { PermissionId = 22, Language = "ru", Name = "Просмотр организаций" },
            new { PermissionId = 22, Language = "az", Name = "Təşkilata baxış" },
            new { PermissionId = 23, Language = "en", Name = "Organization edit" },
            new { PermissionId = 23, Language = "ru", Name = "Редактирование организаций" },
            new { PermissionId = 23, Language = "az", Name = "Təşkilatı redaktə et" },
            new { PermissionId = 24, Language = "en", Name = "Organization delete" },
            new { PermissionId = 24, Language = "ru", Name = "Удаление организаций" },
            new { PermissionId = 24, Language = "az", Name = "Təşkilatı sil" },
            new { PermissionId = 25, Language = "en", Name = "User view" },
            new { PermissionId = 25, Language = "ru", Name = "Просмотр пользователей" },
            new { PermissionId = 25, Language = "az", Name = "İstifadəçiyə baxış" },
            new { PermissionId = 26, Language = "en", Name = "User edit" },
            new { PermissionId = 26, Language = "ru", Name = "Редактирование польз." },
            new { PermissionId = 26, Language = "az", Name = "İstifadəçini redaktə" },
            new { PermissionId = 27, Language = "en", Name = "User delete" },
            new { PermissionId = 27, Language = "ru", Name = "Удаление пользователей" },
            new { PermissionId = 27, Language = "az", Name = "İstifadəçини sil" }
        );
    }
}