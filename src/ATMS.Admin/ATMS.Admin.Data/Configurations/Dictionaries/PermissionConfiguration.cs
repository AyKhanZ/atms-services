using ATMS.Admin.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Dictionaries;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();
        
        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(e => e.Module)
            .HasMaxLength(50)
            .IsRequired();

        
        builder.HasMany(p => p.Translations)
            .WithOne(t => t.Permission)
            .HasForeignKey(t => t.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.HasData(
            // Role
            new { Id = 1, Code = "RoleView", Module = "Role" },
            new { Id = 2, Code = "RoleEdit", Module = "Role" },
            new { Id = 3, Code = "RoleDelete", Module = "Role" },
            // User
            new { Id = 4, Code = "UserView", Module = "User" },
            new { Id = 5, Code = "UserEdit", Module = "User" },
            new { Id = 6, Code = "UserDelete", Module = "User" },
            // Project
            new { Id = 7, Code = "ProjectView", Module = "Project" },
            new { Id = 8, Code = "ProjectEdit", Module = "Project" },
            new { Id = 9, Code = "ProjectDelete", Module = "Project" },
            // Comment
            new { Id = 10, Code = "CommentView", Module = "Comment" },
            new { Id = 11, Code = "CommentEdit", Module = "Comment" },
            new { Id = 12, Code = "CommentDelete", Module = "Comment" },
            // Notification
            new { Id = 13, Code = "NotificationView", Module = "Notification" },
            new { Id = 14, Code = "NotificationEdit", Module = "Notification" },
            new { Id = 15, Code = "NotificationDelete", Module = "Notification" }
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
            // Role
            new { PermissionId = 1, Language = "en", Name = "Role View" },
            new { PermissionId = 1, Language = "en", Name = "Просмотр ролей" },
            new { PermissionId = 1, Language = "en", Name = "Rola baxış" },
            new { PermissionId = 2, Language = "en", Name = "Role edit" },
            new { PermissionId = 2, Language = "en", Name = "Редактирование ролей" },
            new { PermissionId = 2, Language = "en", Name = "Rolu redaktə et" },
            new { PermissionId = 3, Language = "en", Name = "Role delete" },
            new { PermissionId = 3, Language = "en", Name = "Удаление ролей" },
            new { PermissionId = 3, Language = "en", Name = "Rolu sil" },
            // User
            new { PermissionId = 4, Language = "en", Name = "User view" },
            new { PermissionId = 4, Language = "en", Name = "Просмотр пользователей" },
            new { PermissionId = 4, Language = "en", Name = "İstifadəçiyə baxış" },
            new { PermissionId = 5, Language = "en", Name = "User edit" },
            new { PermissionId = 5, Language = "en", Name = "Редактирование пользователей" },
            new { PermissionId = 5, Language = "en", Name = "İstifadəçini redaktə" },
            new { PermissionId = 6, Language = "en", Name = "User delete" },
            new { PermissionId = 6, Language = "en", Name = "Удаление пользователей" },
            new { PermissionId = 6, Language = "en", Name = "İstifadəçini sil" },
            // Project
            new { PermissionId = 7, Language = "en", Name = "Project view" },
            new { PermissionId = 7, Language = "en", Name = "Просмотр проектов" },
            new { PermissionId = 7, Language = "en", Name = "Layihəyə baxış" },
            new { PermissionId = 8, Language = "en", Name = "Project edit" },
            new { PermissionId = 8, Language = "en", Name = "Редактирование проектов" },
            new { PermissionId = 8, Language = "en", Name = "Layihəni redaktə" },
            new { PermissionId = 9, Language = "en", Name = "Project delete" },
            new { PermissionId = 9, Language = "en", Name = "Удаление проектов" },
            new { PermissionId = 9, Language = "en", Name = "Layihəni sil" },
            // Comment
            new { PermissionId = 10, Language = "en", Name = "Comment view" },
            new { PermissionId = 10, Language = "en", Name = "Просмотр комментариев" },
            new { PermissionId = 10, Language = "en", Name = "Şərhə baxış" },
            new { PermissionId = 11, Language = "en", Name = "Comment edit" },
            new { PermissionId = 11, Language = "en", Name = "Редактирование комментариев" },
            new { PermissionId = 11, Language = "en", Name = "Şərhi redaktə" },
            new { PermissionId = 12, Language = "en", Name = "Comment delete" },
            new { PermissionId = 12, Language = "en", Name = "Удаление комментариев" },
            new { PermissionId = 12, Language = "en", Name = "Şərhi sil" },
            // Notification
            new { PermissionId = 13, Language = "en", Name = "Notification view" },
            new { PermissionId = 13, Language = "en", Name = "Просмотр уведомлений" },
            new { PermissionId = 13, Language = "en", Name = "Bildirişə baxış" },
            new { PermissionId = 14, Language = "en", Name = "Notification edit" },
            new { PermissionId = 14, Language = "en", Name = "Редактирование уведомлений" },
            new { PermissionId = 14, Language = "en", Name = "Bildirişi redaktə" },
            new { PermissionId = 15, Language = "en", Name = "Notification delete" },
            new { PermissionId = 15, Language = "en", Name = "Удаление уведомлений" },
            new { PermissionId = 15, Language = "en", Name = "Bildirişi sil" }
        );
    }
}
