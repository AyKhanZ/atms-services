using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class ProjectKindConfiguration : IEntityTypeConfiguration<ProjectKind>
{
    public void Configure(EntityTypeBuilder<ProjectKind> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();


        builder.HasMany(p => p.Translations)
            .WithOne(t => t.ProjectKind)
            .HasForeignKey(t => t.ProjectKindId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasData(
            new { Id = 1, Code = "Support" },
            new { Id = 2, Code = "External" },
            new { Id = 3, Code = "Internal" },
            new { Id = 4, Code = "OneTime" }
        );
    }
}

public class ProjectKindTranslationConfiguration : IEntityTypeConfiguration<ProjectKindTranslation>
{
    public void Configure(EntityTypeBuilder<ProjectKindTranslation> builder)
    {
        builder.HasIndex(t => new { t.ProjectKindId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.HasData(
            // Support
            new { Id = 1, ProjectKindId = 1, Language = "en", Name = "Support" },
            new { Id = 2, ProjectKindId = 1, Language = "ru", Name = "Поддержка" },
            new { Id = 3, ProjectKindId = 1, Language = "az", Name = "Dəstək" },
            // External
            new { Id = 4, ProjectKindId = 2, Language = "en", Name = "External" },
            new { Id = 5, ProjectKindId = 2, Language = "ru", Name = "Внешний" },
            new { Id = 6, ProjectKindId = 2, Language = "az", Name = "Xarici" },
            // Internal
            new { Id = 7, ProjectKindId = 3, Language = "en", Name = "Internal" },
            new { Id = 8, ProjectKindId = 3, Language = "ru", Name = "Внутренний" },
            new { Id = 9, ProjectKindId = 3, Language = "az", Name = "Daxili" },
            // OneTime
            new { Id = 10, ProjectKindId = 4, Language = "en", Name = "One Time" },
            new { Id = 11, ProjectKindId = 4, Language = "ru", Name = "Разовый" },
            new { Id = 12, ProjectKindId = 4, Language = "az", Name = "Birdəfəlik" }
        );
    }
}