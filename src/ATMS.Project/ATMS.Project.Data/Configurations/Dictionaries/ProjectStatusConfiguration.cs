using ATMS.Data.Enums;
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
            new { Id = (int)ProjectStatusEnum.Draft, Code = "Draft" },
            new { Id = (int)ProjectStatusEnum.Active, Code = "Active" },
            new { Id = (int)ProjectStatusEnum.OnReview, Code = "OnReview" },
            new { Id = (int)ProjectStatusEnum.Closed, Code = "Closed" }
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
            new { Id = 1, ProjectStatusId = (int)ProjectStatusEnum.Draft, Language = "en", Name = "Draft" },
            new { Id = 2, ProjectStatusId = (int)ProjectStatusEnum.Draft, Language = "ru", Name = "Черновик" },
            new { Id = 3, ProjectStatusId = (int)ProjectStatusEnum.Draft, Language = "az", Name = "Qaralama" },
            // Active
            new { Id = 4, ProjectStatusId = (int)ProjectStatusEnum.Active, Language = "en", Name = "Active" },
            new { Id = 5, ProjectStatusId = (int)ProjectStatusEnum.Active, Language = "ru", Name = "Активный" },
            new { Id = 6, ProjectStatusId = (int)ProjectStatusEnum.Active, Language = "az", Name = "Aktiv" },
            // InReview
            new { Id = 7, ProjectStatusId = (int)ProjectStatusEnum.OnReview, Language = "en", Name = "In Review" },
            new { Id = 8, ProjectStatusId = (int)ProjectStatusEnum.OnReview, Language = "ru", Name = "На проверке" },
            new { Id = 9, ProjectStatusId = (int)ProjectStatusEnum.OnReview, Language = "az", Name = "Yoxlamada" },
            // Closed
            new { Id = 10, ProjectStatusId = (int)ProjectStatusEnum.Closed, Language = "en", Name = "Closed" },
            new { Id = 11, ProjectStatusId = (int)ProjectStatusEnum.Closed, Language = "ru", Name = "Закрыт" },
            new { Id = 12, ProjectStatusId = (int)ProjectStatusEnum.Closed, Language = "az", Name = "Bağlanıb" }
        );
    }
}