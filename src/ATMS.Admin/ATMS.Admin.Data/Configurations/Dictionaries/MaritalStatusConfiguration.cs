using ATMS.Admin.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Dictionaries;

public class MaritalStatusConfiguration : IEntityTypeConfiguration<MaritalStatus>
{
    public void Configure(EntityTypeBuilder<MaritalStatus> builder)
    {
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        
        builder.HasMany(m => m.Translations)
            .WithOne(t => t.MaritalStatus)
            .HasForeignKey(t => t.MaritalStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.HasData(
            new { Id = 1, Code = "NotSpecified" },
            new { Id = 2, Code = "Single" },
            new { Id = 3, Code = "Married" }
        );
    }
}

public class MaritalStatusTranslationConfiguration : IEntityTypeConfiguration<MaritalStatusTranslation>
{
    public void Configure(EntityTypeBuilder<MaritalStatusTranslation> builder)
    {
        builder.HasIndex(t => new { t.MaritalStatusId, t.Language }).IsUnique();

        builder.Property(t => t.Language).HasMaxLength(2).IsRequired();

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();

        
        builder.HasData(
            new { MaritalStatusId = 1, Language = "en", Name = "Not specified" },
            new { MaritalStatusId = 1, Language = "ru", Name = "Не указано" },
            new { MaritalStatusId = 1, Language = "az", Name = "Göstərilməyib" },
            new { MaritalStatusId = 2, Language = "en", Name = "Single" },
            new { MaritalStatusId = 2, Language = "ru", Name = "Холост" },
            new { MaritalStatusId = 2, Language = "az", Name = "Subay" },
            new { MaritalStatusId = 3, Language = "en", Name = "Married" },
            new { MaritalStatusId = 3, Language = "ru", Name = "Женат" },
            new { MaritalStatusId = 3, Language = "az", Name = "Evli" }
        );
    }
}
