using ATMS.Data.Enums;
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
            new { Id = (int)WorkGroupStatusEnum.Planned, Code = "Planned" },
            new { Id = (int)WorkGroupStatusEnum.Active, Code = "Active" },
            new { Id = (int)WorkGroupStatusEnum.Done, Code = "Done" }
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
            new { Id = 1, WorkGroupStatusId = (int)WorkGroupStatusEnum.Planned, Language = "en", Name = "Planned" },
            new { Id = 2, WorkGroupStatusId = (int)WorkGroupStatusEnum.Planned, Language = "ru", Name = "Запланировано" },
            new { Id = 3, WorkGroupStatusId = (int)WorkGroupStatusEnum.Planned, Language = "az", Name = "Planlaşdırılıb" },
            // Active
            new { Id = 4, WorkGroupStatusId = (int)WorkGroupStatusEnum.Active, Language = "en", Name = "Active" },
            new { Id = 5, WorkGroupStatusId = (int)WorkGroupStatusEnum.Active, Language = "ru", Name = "Активный" },
            new { Id = 6, WorkGroupStatusId = (int)WorkGroupStatusEnum.Active, Language = "az", Name = "Aktiv" },
            // Done
            new { Id = 7, WorkGroupStatusId = (int)WorkGroupStatusEnum.Done, Language = "en", Name = "Done" },
            new { Id = 8, WorkGroupStatusId = (int)WorkGroupStatusEnum.Done, Language = "ru", Name = "Завершено" },
            new { Id = 9, WorkGroupStatusId = (int)WorkGroupStatusEnum.Done, Language = "az", Name = "Bitdi" }
        );
    }
}
