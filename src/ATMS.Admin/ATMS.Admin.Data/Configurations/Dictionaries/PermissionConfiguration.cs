using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data.Constants;
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
            new { Id = PermissionIds.RoleView, Code = "RoleView", Module = "Role" },
            new { Id = PermissionIds.RoleEdit, Code = "RoleEdit", Module = "Role" },
            new { Id = PermissionIds.RoleDelete, Code = "RoleDelete", Module = "Role" },
            // User
            new { Id = PermissionIds.UserView, Code = "UserView", Module = "User" },
            new { Id = PermissionIds.UserEdit, Code = "UserEdit", Module = "User" },
            new { Id = PermissionIds.UserDelete, Code = "UserDelete", Module = "User" },
            // Project
            new { Id = PermissionIds.ProjectView, Code = "ProjectView", Module = "Project" },
            new { Id = PermissionIds.ProjectEdit, Code = "ProjectEdit", Module = "Project" },
            new { Id = PermissionIds.ProjectDelete, Code = "ProjectDelete", Module = "Project" },
            // Notification
            new { Id = PermissionIds.NotificationView, Code = "NotificationView", Module = "Notification" },
            new { Id = PermissionIds.NotificationEdit, Code = "NotificationEdit", Module = "Notification" },
            new { Id = PermissionIds.NotificationDelete, Code = "NotificationDelete", Module = "Notification" },
            // Comment
            new { Id = PermissionIds.CommentView, Code = "CommentView", Module = "Comment" },
            new { Id = PermissionIds.CommentEdit, Code = "CommentEdit", Module = "Comment" },
            new { Id = PermissionIds.CommentDelete, Code = "CommentDelete", Module = "Comment" }
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
            new { Id = 1, PermissionId = PermissionIds.RoleView, Language = "en", Name = "Role View" },
            new { Id = 2, PermissionId = PermissionIds.RoleView, Language = "ru", Name = "Просмотр ролей" },
            new { Id = 3, PermissionId = PermissionIds.RoleView, Language = "az", Name = "Rola baxış" },
            new { Id = 4, PermissionId = PermissionIds.RoleEdit, Language = "en", Name = "Role edit" },
            new { Id = 5, PermissionId = PermissionIds.RoleEdit, Language = "ru", Name = "Редактирование ролей" },
            new { Id = 6, PermissionId = PermissionIds.RoleEdit, Language = "az", Name = "Rolu redaktə et" },
            new { Id = 7, PermissionId = PermissionIds.RoleDelete, Language = "en", Name = "Role delete" },
            new { Id = 8, PermissionId = PermissionIds.RoleDelete, Language = "ru", Name = "Удаление ролей" },
            new { Id = 9, PermissionId = PermissionIds.RoleDelete, Language = "az", Name = "Rolu sil" },
            // User
            new { Id = 10, PermissionId = PermissionIds.UserView, Language = "en", Name = "User view" },
            new { Id = 11, PermissionId = PermissionIds.UserView, Language = "ru", Name = "Просмотр пользователей" },
            new { Id = 12, PermissionId = PermissionIds.UserView, Language = "az", Name = "İstifadəçiyə baxış" },
            new { Id = 13, PermissionId = PermissionIds.UserEdit, Language = "en", Name = "User edit" },
            new { Id = 14, PermissionId = PermissionIds.UserEdit, Language = "ru", Name = "Редактирование пользователей" },
            new { Id = 15, PermissionId = PermissionIds.UserEdit, Language = "az", Name = "İstifadəçini redaktə" },
            new { Id = 16, PermissionId = PermissionIds.UserDelete, Language = "en", Name = "User delete" },
            new { Id = 17, PermissionId = PermissionIds.UserDelete, Language = "ru", Name = "Удаление пользователей" },
            new { Id = 18, PermissionId = PermissionIds.UserDelete, Language = "az", Name = "İstifadəçini sil" },
            // Project
            new { Id = 19, PermissionId = PermissionIds.ProjectView, Language = "en", Name = "Project view" },
            new { Id = 20, PermissionId = PermissionIds.ProjectView, Language = "ru", Name = "Просмотр проектов" },
            new { Id = 21, PermissionId = PermissionIds.ProjectView, Language = "az", Name = "Layihəyə baxış" },
            new { Id = 22, PermissionId = PermissionIds.ProjectEdit, Language = "en", Name = "Project edit" },
            new { Id = 23, PermissionId = PermissionIds.ProjectEdit, Language = "ru", Name = "Редактирование проектов" },
            new { Id = 24, PermissionId = PermissionIds.ProjectEdit, Language = "az", Name = "Layihəni redaktə" },
            new { Id = 25, PermissionId = PermissionIds.ProjectDelete, Language = "en", Name = "Project delete" },
            new { Id = 26, PermissionId = PermissionIds.ProjectDelete, Language = "ru", Name = "Удаление проектов" },
            new { Id = 27, PermissionId = PermissionIds.ProjectDelete, Language = "az", Name = "Layihəni sil" },
            // Comment
            new { Id = 28, PermissionId = PermissionIds.CommentView, Language = "en", Name = "Comment view" },
            new { Id = 29, PermissionId = PermissionIds.CommentView, Language = "ru", Name = "Просмотр комментариев" },
            new { Id = 30, PermissionId = PermissionIds.CommentView, Language = "az", Name = "Şərhə baxış" },
            new { Id = 31, PermissionId = PermissionIds.CommentEdit, Language = "en", Name = "Comment edit" },
            new { Id = 32, PermissionId = PermissionIds.CommentEdit, Language = "ru", Name = "Редактирование комментариев" },
            new { Id = 33, PermissionId = PermissionIds.CommentEdit, Language = "az", Name = "Şərhi redaktə" },
            new { Id = 34, PermissionId = PermissionIds.CommentDelete, Language = "en", Name = "Comment delete" },
            new { Id = 35, PermissionId = PermissionIds.CommentDelete, Language = "ru", Name = "Удаление комментариев" },
            new { Id = 36, PermissionId = PermissionIds.CommentDelete, Language = "az", Name = "Şərhi sil" },
            // Notification
            new { Id = 37, PermissionId = PermissionIds.NotificationView, Language = "en", Name = "Notification view" },
            new { Id = 38, PermissionId = PermissionIds.NotificationView, Language = "ru", Name = "Просмотр уведомлений" },
            new { Id = 39, PermissionId = PermissionIds.NotificationView, Language = "az", Name = "Bildirişə baxış" },
            new { Id = 40, PermissionId = PermissionIds.NotificationEdit, Language = "en", Name = "Notification edit" },
            new { Id = 41, PermissionId = PermissionIds.NotificationEdit, Language = "ru", Name = "Редактирование уведомлений" },
            new { Id = 42, PermissionId = PermissionIds.NotificationEdit, Language = "az", Name = "Bildirişi redaktə" },
            new { Id = 43, PermissionId = PermissionIds.NotificationDelete, Language = "en", Name = "Notification delete" },
            new { Id = 44, PermissionId = PermissionIds.NotificationDelete, Language = "ru", Name = "Удаление уведомлений" },
            new { Id = 45, PermissionId = PermissionIds.NotificationDelete, Language = "az", Name = "Bildirişi sil" }
        );
    }
}
