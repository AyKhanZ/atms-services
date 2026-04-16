using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data.Enums;
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
            new { Id = (int)GenderEnum.NotSpecified, Code = "NotSpecified" },
            new { Id = (int)GenderEnum.Male, Code = "Male" },
            new { Id = (int)GenderEnum.Female, Code = "Female" },
            new { Id = (int)GenderEnum.Other, Code = "Other" }
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
            new { Id = 1, GenderId = (int)GenderEnum.NotSpecified, Language = "en", Name = "Not specified" },
            new { Id = 2, GenderId = (int)GenderEnum.NotSpecified, Language = "ru", Name = "Не указано" },
            new { Id = 3, GenderId = (int)GenderEnum.NotSpecified, Language = "az", Name = "Göstərilməyib" },
            // Male
            new { Id = 4, GenderId = (int)GenderEnum.Male, Language = "en", Name = "Male" },
            new { Id = 5, GenderId = (int)GenderEnum.Male, Language = "ru", Name = "Мужской" },
            new { Id = 6, GenderId = (int)GenderEnum.Male, Language = "az", Name = "Kişi" },
            // Female
            new { Id = 7, GenderId = (int)GenderEnum.Female, Language = "en", Name = "Female" },
            new { Id = 8, GenderId = (int)GenderEnum.Female, Language = "ru", Name = "Женский" },
            new { Id = 9, GenderId = (int)GenderEnum.Female, Language = "az", Name = "Qadın" },
            // Other
            new { Id = 10, GenderId = (int)GenderEnum.Other, Language = "en", Name = "Other" },
            new { Id = 11, GenderId = (int)GenderEnum.Other, Language = "ru", Name = "Другое" },
            new { Id = 12, GenderId = (int)GenderEnum.Other, Language = "az", Name = "Digər" }
        );
    }
}
