using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations.Dictionaries;

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
            new { Id = 1, Code = "Standard" },
            new { Id = 2, Code = "Optimal" },
            new { Id = 3, Code = "Premium" }
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
            new { Id = 1, ProjectTypeId = 1, Language = "en", Name = "Standard" },
            new { Id = 2, ProjectTypeId = 1, Language = "ru", Name = "Стандартный" },
            new { Id = 3, ProjectTypeId = 1, Language = "az", Name = "Standart" },
            // Optimal
            new { Id = 4, ProjectTypeId = 2, Language = "en", Name = "Optimal" },
            new { Id = 5, ProjectTypeId = 2, Language = "ru", Name = "Оптимальный" },
            new { Id = 6, ProjectTypeId = 2, Language = "az", Name = "Optimal" },
            // Premium
            new { Id = 7, ProjectTypeId = 3, Language = "en", Name = "Premium" },
            new { Id = 8, ProjectTypeId = 3, Language = "ru", Name = "Премиум" },
            new { Id = 9, ProjectTypeId = 3, Language = "az", Name = "Premium" }
        );
    }
}