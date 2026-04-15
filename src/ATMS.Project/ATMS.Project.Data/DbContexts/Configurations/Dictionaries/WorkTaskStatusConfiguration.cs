using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations.Dictionaries;

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
            new { Id = 1, Code = "New" },
            new { Id = 2, Code = "InProgress" },
            new { Id = 3, Code = "Done" }
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
            new { Id = 1, WorkTaskStatusId = 1, Language = "en", Name = "New" },
            new { Id = 2, WorkTaskStatusId = 1, Language = "ru", Name = "Новый" },
            new { Id = 3, WorkTaskStatusId = 1, Language = "az", Name = "Yeni" },
            // InProgress
            new { Id = 4, WorkTaskStatusId = 2, Language = "en", Name = "In Progress" },
            new { Id = 5, WorkTaskStatusId = 2, Language = "ru", Name = "В работе" },
            new { Id = 6, WorkTaskStatusId = 2, Language = "az", Name = "İşdə" },
            // Done
            new { Id = 7, WorkTaskStatusId = 3, Language = "en", Name = "Done" },
            new { Id = 8, WorkTaskStatusId = 3, Language = "ru", Name = "Выполнено" },
            new { Id = 9, WorkTaskStatusId = 3, Language = "az", Name = "Hazır" }
        );
    }
}
