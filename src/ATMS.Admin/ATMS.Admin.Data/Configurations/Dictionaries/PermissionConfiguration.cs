using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
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
            new { Id = (int)PermissionEnum.RoleView, Code = "RoleView", Module = "Role" },
            new { Id = (int)PermissionEnum.RoleEdit, Code = "RoleEdit", Module = "Role" },
            new { Id = (int)PermissionEnum.RoleDelete, Code = "RoleDelete", Module = "Role" },
            // User
            new { Id = (int)PermissionEnum.UserView, Code = "UserView", Module = "User" },
            new { Id = (int)PermissionEnum.UserEdit, Code = "UserEdit", Module = "User" },
            new { Id = (int)PermissionEnum.UserDelete, Code = "UserDelete", Module = "User" },
            // Project
            new { Id = (int)PermissionEnum.ProjectView, Code = "ProjectView", Module = "Project" },
            new { Id = (int)PermissionEnum.ProjectEdit, Code = "ProjectEdit", Module = "Project" },
            new { Id = (int)PermissionEnum.ProjectDelete, Code = "ProjectDelete", Module = "Project" },
            // Notification
            new { Id = (int)PermissionEnum.NotificationView, Code = "NotificationView", Module = "Notification" },
            new { Id = (int)PermissionEnum.NotificationEdit, Code = "NotificationEdit", Module = "Notification" },
            new { Id = (int)PermissionEnum.NotificationDelete, Code = "NotificationDelete", Module = "Notification" },
            // Comment
            new { Id = (int)PermissionEnum.CommentView, Code = "CommentView", Module = "Comment" },
            new { Id = (int)PermissionEnum.CommentEdit, Code = "CommentEdit", Module = "Comment" },
            new { Id = (int)PermissionEnum.CommentDelete, Code = "CommentDelete", Module = "Comment" }
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
            new { Id = 1, PermissionId = (int)PermissionEnum.RoleView, Language = "en", Name = "Role View" },
            new { Id = 2, PermissionId = (int)PermissionEnum.RoleView, Language = "ru", Name = "Просмотр ролей" },
            new { Id = 3, PermissionId = (int)PermissionEnum.RoleView, Language = "az", Name = "Rola baxış" },
            new { Id = 4, PermissionId = (int)PermissionEnum.RoleEdit, Language = "en", Name = "Role edit" },
            new { Id = 5, PermissionId = (int)PermissionEnum.RoleEdit, Language = "ru", Name = "Редактирование ролей" },
            new { Id = 6, PermissionId = (int)PermissionEnum.RoleEdit, Language = "az", Name = "Rolu redaktə et" },
            new { Id = 7, PermissionId = (int)PermissionEnum.RoleDelete, Language = "en", Name = "Role delete" },
            new { Id = 8, PermissionId = (int)PermissionEnum.RoleDelete, Language = "ru", Name = "Удаление ролей" },
            new { Id = 9, PermissionId = (int)PermissionEnum.RoleDelete, Language = "az", Name = "Rolu sil" },
            // User
            new { Id = 10, PermissionId = (int)PermissionEnum.UserView, Language = "en", Name = "User view" },
            new { Id = 11, PermissionId = (int)PermissionEnum.UserView, Language = "ru", Name = "Просмотр пользователей" },
            new { Id = 12, PermissionId = (int)PermissionEnum.UserView, Language = "az", Name = "İstifadəçiyə baxış" },
            new { Id = 13, PermissionId = (int)PermissionEnum.UserEdit, Language = "en", Name = "User edit" },
            new { Id = 14, PermissionId = (int)PermissionEnum.UserEdit, Language = "ru", Name = "Редактирование пользователей" },
            new { Id = 15, PermissionId = (int)PermissionEnum.UserEdit, Language = "az", Name = "İstifadəçini redaktə" },
            new { Id = 16, PermissionId = (int)PermissionEnum.UserDelete, Language = "en", Name = "User delete" },
            new { Id = 17, PermissionId = (int)PermissionEnum.UserDelete, Language = "ru", Name = "Удаление пользователей" },
            new { Id = 18, PermissionId = (int)PermissionEnum.UserDelete, Language = "az", Name = "İstifadəçini sil" },
            // Project
            new { Id = 19, PermissionId = (int)PermissionEnum.ProjectView, Language = "en", Name = "Project view" },
            new { Id = 20, PermissionId = (int)PermissionEnum.ProjectView, Language = "ru", Name = "Просмотр проектов" },
            new { Id = 21, PermissionId = (int)PermissionEnum.ProjectView, Language = "az", Name = "Layihəyə baxış" },
            new { Id = 22, PermissionId = (int)PermissionEnum.ProjectEdit, Language = "en", Name = "Project edit" },
            new { Id = 23, PermissionId = (int)PermissionEnum.ProjectEdit, Language = "ru", Name = "Редактирование проектов" },
            new { Id = 24, PermissionId = (int)PermissionEnum.ProjectEdit, Language = "az", Name = "Layihəni redaktə" },
            new { Id = 25, PermissionId = (int)PermissionEnum.ProjectDelete, Language = "en", Name = "Project delete" },
            new { Id = 26, PermissionId = (int)PermissionEnum.ProjectDelete, Language = "ru", Name = "Удаление проектов" },
            new { Id = 27, PermissionId = (int)PermissionEnum.ProjectDelete, Language = "az", Name = "Layihəni sil" },
            // Comment
            new { Id = 28, PermissionId = (int)PermissionEnum.CommentView, Language = "en", Name = "Comment view" },
            new { Id = 29, PermissionId = (int)PermissionEnum.CommentView, Language = "ru", Name = "Просмотр комментариев" },
            new { Id = 30, PermissionId = (int)PermissionEnum.CommentView, Language = "az", Name = "Şərhə baxış" },
            new { Id = 31, PermissionId = (int)PermissionEnum.CommentEdit, Language = "en", Name = "Comment edit" },
            new { Id = 32, PermissionId = (int)PermissionEnum.CommentEdit, Language = "ru", Name = "Редактирование комментариев" },
            new { Id = 33, PermissionId = (int)PermissionEnum.CommentEdit, Language = "az", Name = "Şərhi redaktə" },
            new { Id = 34, PermissionId = (int)PermissionEnum.CommentDelete, Language = "en", Name = "Comment delete" },
            new { Id = 35, PermissionId = (int)PermissionEnum.CommentDelete, Language = "ru", Name = "Удаление комментариев" },
            new { Id = 36, PermissionId = (int)PermissionEnum.CommentDelete, Language = "az", Name = "Şərhi sil" },
            // Notification
            new { Id = 37, PermissionId = (int)PermissionEnum.NotificationView, Language = "en", Name = "Notification view" },
            new { Id = 38, PermissionId = (int)PermissionEnum.NotificationView, Language = "ru", Name = "Просмотр уведомлений" },
            new { Id = 39, PermissionId = (int)PermissionEnum.NotificationView, Language = "az", Name = "Bildirişə baxış" },
            new { Id = 40, PermissionId = (int)PermissionEnum.NotificationEdit, Language = "en", Name = "Notification edit" },
            new { Id = 41, PermissionId = (int)PermissionEnum.NotificationEdit, Language = "ru", Name = "Редактирование уведомлений" },
            new { Id = 42, PermissionId = (int)PermissionEnum.NotificationEdit, Language = "az", Name = "Bildirişi redaktə" },
            new { Id = 43, PermissionId = (int)PermissionEnum.NotificationDelete, Language = "en", Name = "Notification delete" },
            new { Id = 44, PermissionId = (int)PermissionEnum.NotificationDelete, Language = "ru", Name = "Удаление уведомлений" },
            new { Id = 45, PermissionId = (int)PermissionEnum.NotificationDelete, Language = "az", Name = "Bildirişi sil" }
        );
    }
}
