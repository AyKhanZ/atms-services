using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Dictionaries;

public class MaritalStatusConfiguration : IEntityTypeConfiguration<MaritalStatus>
{
    public void Configure(EntityTypeBuilder<MaritalStatus> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        
        builder.HasMany(m => m.Translations)
            .WithOne(t => t.MaritalStatus)
            .HasForeignKey(t => t.MaritalStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.HasData(
            new { Id = (int)MaritalStatusEnum.NotSpecified, Code = "NotSpecified" },
            new { Id = (int)MaritalStatusEnum.Single, Code = "Single" },
            new { Id = (int)MaritalStatusEnum.Married, Code = "Married" }
        );
    }
}

public class MaritalStatusTranslationConfiguration : IEntityTypeConfiguration<MaritalStatusTranslation>
{
    public void Configure(EntityTypeBuilder<MaritalStatusTranslation> builder)
    {
        builder.HasIndex(t => new { t.MaritalStatusId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        
        builder.HasData(
            // NotSpecified
            new { Id = 1, MaritalStatusId = (int)MaritalStatusEnum.NotSpecified, Language = "en", Name = "Not specified" },
            new { Id = 2, MaritalStatusId = (int)MaritalStatusEnum.NotSpecified, Language = "ru", Name = "Не указано" },
            new { Id = 3, MaritalStatusId = (int)MaritalStatusEnum.NotSpecified, Language = "az", Name = "Göstərilməyib" },
            // Single
            new { Id = 4, MaritalStatusId = (int)MaritalStatusEnum.Single, Language = "en", Name = "Single" },
            new { Id = 5, MaritalStatusId = (int)MaritalStatusEnum.Single, Language = "ru", Name = "Холост" },
            new { Id = 6, MaritalStatusId = (int)MaritalStatusEnum.Single, Language = "az", Name = "Subay" },
            // Married
            new { Id = 7, MaritalStatusId = (int)MaritalStatusEnum.Married, Language = "en", Name = "Married" },
            new { Id = 8, MaritalStatusId = (int)MaritalStatusEnum.Married, Language = "ru", Name = "Женат" },
            new { Id = 9, MaritalStatusId = (int)MaritalStatusEnum.Married, Language = "az", Name = "Evli" }
        );
    }
}
