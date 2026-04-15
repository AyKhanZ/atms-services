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
            new { Id = 1, Code = "Low" },
            new { Id = 2, Code = "Medium" },
            new { Id = 3, Code = "High" }
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
            new { Id = 1, WorkItemPriorityId = 1, Language = "en", Name = "Low" },
            new { Id = 2, WorkItemPriorityId = 1, Language = "ru", Name = "Низкий" },
            new { Id = 3, WorkItemPriorityId = 1, Language = "az", Name = "Aşağı" },
            // Medium
            new { Id = 4, WorkItemPriorityId = 2, Language = "en", Name = "Medium" },
            new { Id = 5, WorkItemPriorityId = 2, Language = "ru", Name = "Средний" },
            new { Id = 6, WorkItemPriorityId = 2, Language = "az", Name = "Orta" },
            // High
            new { Id = 7, WorkItemPriorityId = 3, Language = "en", Name = "High" },
            new { Id = 8, WorkItemPriorityId = 3, Language = "ru", Name = "Высокий" },
            new { Id = 9, WorkItemPriorityId = 3, Language = "az", Name = "Yüksək" }
        );
    }
}
