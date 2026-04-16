using ATMS.Data.Enums;
using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class WorkItemPriorityConfiguration : IEntityTypeConfiguration<WorkItemPriority>
{
    public void Configure(EntityTypeBuilder<WorkItemPriority> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(p => p.Translations)
            .WithOne(t => t.WorkItemPriority)
            .HasForeignKey(t => t.WorkItemPriorityId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasData(
            new { Id = (int)WorkItemPriorityEnum.Low, Code = "Low" },
            new { Id = (int)WorkItemPriorityEnum.Medium, Code = "Medium" },
            new { Id = (int)WorkItemPriorityEnum.High, Code = "High" }
        );
    }
}

public class WorkItemPriorityTranslationConfiguration : IEntityTypeConfiguration<WorkItemPriorityTranslation>
{
    public void Configure(EntityTypeBuilder<WorkItemPriorityTranslation> builder)
    {
        builder.HasIndex(t => new { t.WorkItemPriorityId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.HasData(
            // Low
            new { Id = 1, WorkItemPriorityId = (int)WorkItemPriorityEnum.Low, Language = "en", Name = "Low" },
            new { Id = 2, WorkItemPriorityId = (int)WorkItemPriorityEnum.Low, Language = "ru", Name = "Низкий" },
            new { Id = 3, WorkItemPriorityId = (int)WorkItemPriorityEnum.Low, Language = "az", Name = "Aşağı" },
            // Medium
            new { Id = 4, WorkItemPriorityId = (int)WorkItemPriorityEnum.Medium, Language = "en", Name = "Medium" },
            new { Id = 5, WorkItemPriorityId = (int)WorkItemPriorityEnum.Medium, Language = "ru", Name = "Средний" },
            new { Id = 6, WorkItemPriorityId = (int)WorkItemPriorityEnum.Medium, Language = "az", Name = "Orta" },
            // High
            new { Id = 7, WorkItemPriorityId = (int)WorkItemPriorityEnum.High, Language = "en", Name = "High" },
            new { Id = 8, WorkItemPriorityId = (int)WorkItemPriorityEnum.High, Language = "ru", Name = "Высокий" },
            new { Id = 9, WorkItemPriorityId = (int)WorkItemPriorityEnum.High, Language = "az", Name = "Yüksək" }
        );
    }
}
