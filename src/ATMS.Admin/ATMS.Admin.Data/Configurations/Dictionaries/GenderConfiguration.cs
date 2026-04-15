using ATMS.Admin.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Dictionaries;

public class GenderConfiguration : IEntityTypeConfiguration<Gender>
{
    public void Configure(EntityTypeBuilder<Gender> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        
        builder.HasMany(g => g.Translations)
            .WithOne(t => t.Gender)
            .HasForeignKey(t => t.GenderId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.HasData(
            new { Id = 1, Code = "NotSpecified" },
            new { Id = 2, Code = "Male" },
            new { Id = 3, Code = "Female" },
            new { Id = 4, Code = "Other" }
        );
    }
}

public class GenderTranslationConfiguration : IEntityTypeConfiguration<GenderTranslation>
{
    public void Configure(EntityTypeBuilder<GenderTranslation> builder)
    {
        builder.HasIndex(t => new { t.GenderId, t.Language })
            .IsUnique();
        
        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();
        
        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        
        builder.HasData(
            // NotSpecified
            new { Id = 1, GenderId = 1, Language = "en", Name = "Not specified" },
            new { Id = 2, GenderId = 1, Language = "ru", Name = "Не указано" },
            new { Id = 3, GenderId = 1, Language = "az", Name = "Göstərilməyib" },
            // Male
            new { Id = 4, GenderId = 2, Language = "en", Name = "Male" },
            new { Id = 5, GenderId = 2, Language = "ru", Name = "Мужской" },
            new { Id = 6, GenderId = 2, Language = "az", Name = "Kişi" },
            // Female
            new { Id = 7, GenderId = 3, Language = "en", Name = "Female" },
            new { Id = 8, GenderId = 3, Language = "ru", Name = "Женский" },
            new { Id = 9, GenderId = 3, Language = "az", Name = "Qadın" },
            // Other
            new { Id = 10, GenderId = 4, Language = "en", Name = "Other" },
            new { Id = 11, GenderId = 4, Language = "ru", Name = "Другое" },
            new { Id = 12, GenderId = 4, Language = "az", Name = "Digər" }
        );
    }
}
