using ATMS.Admin.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Dictionaries;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Code)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.NativeName)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasData(
            new Language { Id = 1, Code = "AZ", Name = "Azerbaijani", NativeName = "Azərbaycanca" },
            new Language { Id = 2, Code = "EN", Name = "English", NativeName = "English" },
            new Language { Id = 3, Code = "RU", Name = "Russian", NativeName = "Русский" });
    }
}
