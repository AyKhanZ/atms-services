using ATMS.Data.Enums;
using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class ProjectTypeConfiguration : IEntityTypeConfiguration<ProjectType>
{
    public void Configure(EntityTypeBuilder<ProjectType> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();


        builder.HasMany(p => p.Translations)
            .WithOne(t => t.ProjectType)
            .HasForeignKey(t => t.ProjectTypeId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasData(
            new { Id = (int)ProjectTypeEnum.Standard, Code = "Standard" },
            new { Id = (int)ProjectTypeEnum.Optimal, Code = "Optimal" },
            new { Id = (int)ProjectTypeEnum.Premium, Code = "Premium" }
        );
    }
}

public class ProjectTypeTranslationConfiguration : IEntityTypeConfiguration<ProjectTypeTranslation>
{
    public void Configure(EntityTypeBuilder<ProjectTypeTranslation> builder)
    {
        builder.HasIndex(t => new { t.ProjectTypeId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.HasData(
            // Standard
            new { Id = 1, ProjectTypeId = (int)ProjectTypeEnum.Standard, Language = "en", Name = "Standard" },
            new { Id = 2, ProjectTypeId = (int)ProjectTypeEnum.Standard, Language = "ru", Name = "Стандартный" },
            new { Id = 3, ProjectTypeId = (int)ProjectTypeEnum.Standard, Language = "az", Name = "Standart" },
            // Optimal
            new { Id = 4, ProjectTypeId = (int)ProjectTypeEnum.Optimal, Language = "en", Name = "Optimal" },
            new { Id = 5, ProjectTypeId = (int)ProjectTypeEnum.Optimal, Language = "ru", Name = "Оптимальный" },
            new { Id = 6, ProjectTypeId = (int)ProjectTypeEnum.Optimal, Language = "az", Name = "Optimal" },
            // Premium
            new { Id = 7, ProjectTypeId = (int)ProjectTypeEnum.Premium, Language = "en", Name = "Premium" },
            new { Id = 8, ProjectTypeId = (int)ProjectTypeEnum.Premium, Language = "ru", Name = "Премиум" },
            new { Id = 9, ProjectTypeId = (int)ProjectTypeEnum.Premium, Language = "az", Name = "Premium" }
        );
    }
}