using ATMS.Data.Enums;
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
            new { Id = (int)ProjectKindEnum.Support, Code = "Support" },
            new { Id = (int)ProjectKindEnum.External, Code = "External" },
            new { Id = (int)ProjectKindEnum.Internal, Code = "Internal" },
            new { Id = (int)ProjectKindEnum.OneTime, Code = "OneTime" }
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
            new { Id = 1, ProjectKindId = (int)ProjectKindEnum.Support, Language = "en", Name = "Support" },
            new { Id = 2, ProjectKindId = (int)ProjectKindEnum.Support, Language = "ru", Name = "Поддержка" },
            new { Id = 3, ProjectKindId = (int)ProjectKindEnum.Support, Language = "az", Name = "Dəstək" },
            // External
            new { Id = 4, ProjectKindId = (int)ProjectKindEnum.External, Language = "en", Name = "External" },
            new { Id = 5, ProjectKindId = (int)ProjectKindEnum.External, Language = "ru", Name = "Внешний" },
            new { Id = 6, ProjectKindId = (int)ProjectKindEnum.External, Language = "az", Name = "Xarici" },
            // Internal
            new { Id = 7, ProjectKindId = (int)ProjectKindEnum.Internal, Language = "en", Name = "Internal" },
            new { Id = 8, ProjectKindId = (int)ProjectKindEnum.Internal, Language = "ru", Name = "Внутренний" },
            new { Id = 9, ProjectKindId = (int)ProjectKindEnum.Internal, Language = "az", Name = "Daxili" },
            // OneTime
            new { Id = 10, ProjectKindId = (int)ProjectKindEnum.OneTime, Language = "en", Name = "One Time" },
            new { Id = 11, ProjectKindId = (int)ProjectKindEnum.OneTime, Language = "ru", Name = "Разовый" },
            new { Id = 12, ProjectKindId = (int)ProjectKindEnum.OneTime, Language = "az", Name = "Birdəfəlik" }
        );
    }
}