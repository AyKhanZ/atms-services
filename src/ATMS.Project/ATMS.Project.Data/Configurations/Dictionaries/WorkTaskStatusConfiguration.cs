using ATMS.Data.Enums;
using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class WorkTaskStatusConfiguration : IEntityTypeConfiguration<WorkTaskStatus>
{
    public void Configure(EntityTypeBuilder<WorkTaskStatus> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(p => p.Translations)
            .WithOne(t => t.WorkTaskStatus)
            .HasForeignKey(t => t.WorkTaskStatusId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
        builder.HasData(
            new { Id = (int)WorkTaskStatusEnum.New, Code = "New" },
            new { Id = (int)WorkTaskStatusEnum.InProgress, Code = "InProgress" },
            new { Id = (int)WorkTaskStatusEnum.Done, Code = "Done" }
        );
    }
}

public class WorkTaskStatusTranslationConfiguration : IEntityTypeConfiguration<WorkTaskStatusTranslation>
{
    public void Configure(EntityTypeBuilder<WorkTaskStatusTranslation> builder)
    {
        builder.HasIndex(t => new { t.WorkTaskStatusId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        
        builder.HasData(
            // New
            new { Id = 1, WorkTaskStatusId = (int)WorkTaskStatusEnum.New, Language = "en", Name = "New" },
            new { Id = 2, WorkTaskStatusId = (int)WorkTaskStatusEnum.New, Language = "ru", Name = "Новый" },
            new { Id = 3, WorkTaskStatusId = (int)WorkTaskStatusEnum.New, Language = "az", Name = "Yeni" },
            // InProgress
            new { Id = 4, WorkTaskStatusId = (int)WorkTaskStatusEnum.InProgress, Language = "en", Name = "In Progress" },
            new { Id = 5, WorkTaskStatusId = (int)WorkTaskStatusEnum.InProgress, Language = "ru", Name = "В работе" },
            new { Id = 6, WorkTaskStatusId = (int)WorkTaskStatusEnum.InProgress, Language = "az", Name = "İşdə" },
            // Done
            new { Id = 7, WorkTaskStatusId = (int)WorkTaskStatusEnum.Done, Language = "en", Name = "Done" },
            new { Id = 8, WorkTaskStatusId = (int)WorkTaskStatusEnum.Done, Language = "ru", Name = "Выполнено" },
            new { Id = 9, WorkTaskStatusId = (int)WorkTaskStatusEnum.Done, Language = "az", Name = "Hazır" }
        );
    }
}
