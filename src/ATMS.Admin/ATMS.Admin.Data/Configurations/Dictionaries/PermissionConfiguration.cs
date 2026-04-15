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
            new { Id = 1, PermissionId = 1, Language = "en", Name = "Role View" },
            new { Id = 2, PermissionId = 1, Language = "en", Name = "Просмотр ролей" },
            new { Id = 3, PermissionId = 1, Language = "en", Name = "Rola baxış" },
            new { Id = 4, PermissionId = 2, Language = "en", Name = "Role edit" },
            new { Id = 5, PermissionId = 2, Language = "en", Name = "Редактирование ролей" },
            new { Id = 6, PermissionId = 2, Language = "en", Name = "Rolu redaktə et" },
            new { Id = 7, PermissionId = 3, Language = "en", Name = "Role delete" },
            new { Id = 8, PermissionId = 3, Language = "en", Name = "Удаление ролей" },
            new { Id = 9, PermissionId = 3, Language = "en", Name = "Rolu sil" },
            // User
            new { Id = 10, PermissionId = 4, Language = "en", Name = "User view" },
            new { Id = 11, PermissionId = 4, Language = "en", Name = "Просмотр пользователей" },
            new { Id = 12, PermissionId = 4, Language = "en", Name = "İstifadəçiyə baxış" },
            new { Id = 13, PermissionId = 5, Language = "en", Name = "User edit" },
            new { Id = 14, PermissionId = 5, Language = "en", Name = "Редактирование пользователей" },
            new { Id = 15, PermissionId = 5, Language = "en", Name = "İstifadəçini redaktə" },
            new { Id = 16, PermissionId = 6, Language = "en", Name = "User delete" },
            new { Id = 17, PermissionId = 6, Language = "en", Name = "Удаление пользователей" },
            new { Id = 18, PermissionId = 6, Language = "en", Name = "İstifadəçini sil" },
            // Project
            new { Id = 19, PermissionId = 7, Language = "en", Name = "Project view" },
            new { Id = 20, PermissionId = 7, Language = "en", Name = "Просмотр проектов" },
            new { Id = 21, PermissionId = 7, Language = "en", Name = "Layihəyə baxış" },
            new { Id = 22, PermissionId = 8, Language = "en", Name = "Project edit" },
            new { Id = 23, PermissionId = 8, Language = "en", Name = "Редактирование проектов" },
            new { Id = 24, PermissionId = 8, Language = "en", Name = "Layihəni redaktə" },
            new { Id = 25, PermissionId = 9, Language = "en", Name = "Project delete" },
            new { Id = 26, PermissionId = 9, Language = "en", Name = "Удаление проектов" },
            new { Id = 27, PermissionId = 9, Language = "en", Name = "Layihəni sil" },
            // Comment
            new { Id = 28, PermissionId = 10, Language = "en", Name = "Comment view" },
            new { Id = 29, PermissionId = 10, Language = "en", Name = "Просмотр комментариев" },
            new { Id = 30, PermissionId = 10, Language = "en", Name = "Şərhə baxış" },
            new { Id = 31, PermissionId = 11, Language = "en", Name = "Comment edit" },
            new { Id = 32, PermissionId = 11, Language = "en", Name = "Редактирование комментариев" },
            new { Id = 33, PermissionId = 11, Language = "en", Name = "Şərhi redaktə" },
            new { Id = 34, PermissionId = 12, Language = "en", Name = "Comment delete" },
            new { Id = 35, PermissionId = 12, Language = "en", Name = "Удаление комментариев" },
            new { Id = 36, PermissionId = 12, Language = "en", Name = "Şərhi sil" },
            // Notification
            new { Id = 37, PermissionId = 13, Language = "en", Name = "Notification view" },
            new { Id = 38, PermissionId = 13, Language = "en", Name = "Просмотр уведомлений" },
            new { Id = 39, PermissionId = 13, Language = "en", Name = "Bildirişə baxış" },
            new { Id = 40, PermissionId = 14, Language = "en", Name = "Notification edit" },
            new { Id = 41, PermissionId = 14, Language = "en", Name = "Редактирование уведомлений" },
            new { Id = 42, PermissionId = 14, Language = "en", Name = "Bildirişi redaktə" },
            new { Id = 43, PermissionId = 15, Language = "en", Name = "Notification delete" },
            new { Id = 44, PermissionId = 15, Language = "en", Name = "Удаление уведомлений" },
            new { Id = 45, PermissionId = 15, Language = "en", Name = "Bildirişi sil" }
        );
    }
}
