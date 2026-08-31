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

        builder.HasData(ProjectPermissionSeed.Definitions
            .Select(definition => new
            {
                Id = (int)definition.Permission,
                Code = definition.Permission.ToString()
            })
            .Cast<object>()
            .ToArray());
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

        builder.HasData(ProjectPermissionSeed.Definitions
            .SelectMany(definition => new object[]
            {
                new
                {
                    Id = definition.TranslationId,
                    PermissionId = (int)definition.Permission,
                    Language = "en",
                    Name = definition.English
                },
                new
                {
                    Id = definition.TranslationId + 1,
                    PermissionId = (int)definition.Permission,
                    Language = "ru",
                    Name = definition.Russian
                },
                new
                {
                    Id = definition.TranslationId + 2,
                    PermissionId = (int)definition.Permission,
                    Language = "az",
                    Name = definition.Azerbaijani
                }
            })
            .ToArray());
    }
}

internal static class ProjectPermissionSeed
{
    internal static readonly ProjectPermissionDefinition[] Definitions =
    [
        new(ProjectPermissionEnum.ProjectView, 1, "Project view", "Просмотр проекта", "Layihəyə baxış"),
        new(ProjectPermissionEnum.ProjectEdit, 4, "Project edit", "Редактирование проекта", "Layihəni redaktə et"),
        new(ProjectPermissionEnum.TicketEdit, 13, "Ticket edit", "Редактирование тикетов", "Tiketi redaktə et"),
        new(ProjectPermissionEnum.TicketDelete, 16, "Ticket delete", "Удаление тикетов", "Tiketi sil"),
        new(ProjectPermissionEnum.TaskEdit, 22, "Task edit", "Редактирование задач", "Tapşırığı redaktə et"),
        new(ProjectPermissionEnum.TaskDelete, 25, "Task delete", "Удаление задач", "Tapşırığı sil"),
        new(ProjectPermissionEnum.CommentEdit, 31, "Comment edit", "Редактирование комментариев", "Şərhi redaktə et"),
        new(ProjectPermissionEnum.CommentDelete, 34, "Comment delete", "Удаление комментариев", "Şərhi sil"),
        new(ProjectPermissionEnum.NotificationEdit, 40, "Notification edit", "Редактирование уведомлений", "Bildirişi redaktə et"),
        new(ProjectPermissionEnum.NotificationDelete, 43, "Notification delete", "Удаление уведомлений", "Bildirişi sil"),
        new(ProjectPermissionEnum.ParticipantEdit, 76, "Participant edit", "Изменение роли участника", "İştirakçını redaktə et"),
        new(ProjectPermissionEnum.ParticipantDelete, 79, "Participant delete", "Удаление участника", "İştirakçını sil"),
        new(ProjectPermissionEnum.ParticipantInviteClient, 82, "Invite client participant", "Приглашение участника клиента", "Müştəri iştirakçısını dəvət et"),
        new(ProjectPermissionEnum.ParticipantInviteEmployee, 97, "Invite employee participant", "Приглашение сотрудника", "Əməkdaş iştirakçını dəvət et"),
        new(ProjectPermissionEnum.TicketCreate, 85, "Ticket create", "Создание тикетов", "Tiket yarat"),
        new(ProjectPermissionEnum.TaskCreate, 88, "Task create", "Создание задач", "Tapşırıq yarat")
    ];
}

internal sealed record ProjectPermissionDefinition(
    ProjectPermissionEnum Permission,
    int TranslationId,
    string English,
    string Russian,
    string Azerbaijani);
