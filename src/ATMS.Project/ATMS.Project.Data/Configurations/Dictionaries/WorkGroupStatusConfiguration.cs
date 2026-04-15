using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class WorkGroupStatusConfiguration : IEntityTypeConfiguration<WorkGroupStatus>
{
    public void Configure(EntityTypeBuilder<WorkGroupStatus> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();


        builder.HasMany(p => p.Translations)
            .WithOne(t => t.WorkGroupStatus)
            .HasForeignKey(t => t.WorkGroupStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.HasData(
            new { Id = 1, Code = "Planned" },
            new { Id = 2, Code = "Active" },
            new { Id = 3, Code = "Done" }
        );
    }
}

public class WorkGroupStatusTranslationConfiguration : IEntityTypeConfiguration<WorkGroupStatusTranslation>
{
    public void Configure(EntityTypeBuilder<WorkGroupStatusTranslation> builder)
    {
        builder.HasIndex(t => new { t.WorkGroupStatusId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.HasData(
            // Planned
            new { Id = 1, WorkGroupStatusId = 1, Language = "en", Name = "Planned" },
            new { Id = 2, WorkGroupStatusId = 1, Language = "ru", Name = "Запланировано" },
            new { Id = 3, WorkGroupStatusId = 1, Language = "az", Name = "Planlaşdırılıb" },
            // Active
            new { Id = 4, WorkGroupStatusId = 2, Language = "en", Name = "Active" },
            new { Id = 5, WorkGroupStatusId = 2, Language = "ru", Name = "Активный" },
            new { Id = 6, WorkGroupStatusId = 2, Language = "az", Name = "Aktiv" },
            // Done
            new { Id = 7, WorkGroupStatusId = 3, Language = "en", Name = "Done" },
            new { Id = 8, WorkGroupStatusId = 3, Language = "ru", Name = "Завершено" },
            new { Id = 9, WorkGroupStatusId = 3, Language = "az", Name = "Bitdi" }
        );
    }
}
