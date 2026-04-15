using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class ProjectStatusConfiguration : IEntityTypeConfiguration<ProjectStatus>
{
    public void Configure(EntityTypeBuilder<ProjectStatus> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();


        builder.HasMany(p => p.Translations)
            .WithOne(t => t.ProjectStatus)
            .HasForeignKey(t => t.ProjectStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new { Id = 1, Code = "Draft" },
            new { Id = 2, Code = "Active" },
            new { Id = 3, Code = "OnReview" },
            new { Id = 4, Code = "Closed" }
        );
    }
}

public class ProjectStatusTranslationConfiguration : IEntityTypeConfiguration<ProjectStatusTranslation>
{
    public void Configure(EntityTypeBuilder<ProjectStatusTranslation> builder)
    {
        builder.HasIndex(t => new { t.ProjectStatusId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.HasData(
            // Draft
            new { Id = 1, ProjectStatusId = 1, Language = "en", Name = "Draft" },
            new { Id = 2, ProjectStatusId = 1, Language = "ru", Name = "Черновик" },
            new { Id = 3, ProjectStatusId = 1, Language = "az", Name = "Qaralama" },
            // Active
            new { Id = 4, ProjectStatusId = 2, Language = "en", Name = "Active" },
            new { Id = 5, ProjectStatusId = 2, Language = "ru", Name = "Активный" },
            new { Id = 6, ProjectStatusId = 2, Language = "az", Name = "Aktiv" },
            // InReview
            new { Id = 7, ProjectStatusId = 3, Language = "en", Name = "In Review" },
            new { Id = 8, ProjectStatusId = 3, Language = "ru", Name = "На проверке" },
            new { Id = 9, ProjectStatusId = 3, Language = "az", Name = "Yoxlamada" },
            // Closed
            new { Id = 10, ProjectStatusId = 4, Language = "en", Name = "Closed" },
            new { Id = 11, ProjectStatusId = 4, Language = "ru", Name = "Закрыт" },
            new { Id = 12, ProjectStatusId = 4, Language = "az", Name = "Bağlanıb" }
        );
    }
}