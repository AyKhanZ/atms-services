using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public sealed class BoardColumnTranslationConfiguration : IEntityTypeConfiguration<BoardColumnTranslation>
{
    public void Configure(EntityTypeBuilder<BoardColumnTranslation> builder)
    {
        builder.ToTable("BoardColumnTranslations");

        builder.HasIndex(translation => new { translation.BoardColumnId, translation.Language })
            .IsUnique();

        builder.Property(translation => translation.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(translation => translation.Name)
            .HasMaxLength(50)
            .IsRequired();

        // Doing and Done are read under their status on the board ("In Progress: Doing")
        builder.HasData(
            // New
            new { Id = 1, BoardColumnId = 1, Language = "en", Name = "New" },
            new { Id = 2, BoardColumnId = 1, Language = "ru", Name = "Новый" },
            new { Id = 3, BoardColumnId = 1, Language = "az", Name = "Yeni" },
            // In Progress: Doing
            new { Id = 4, BoardColumnId = 2, Language = "en", Name = "Doing" },
            new { Id = 5, BoardColumnId = 2, Language = "ru", Name = "В процессе" },
            new { Id = 6, BoardColumnId = 2, Language = "az", Name = "İcrada" },
            // In Progress: Done
            new { Id = 7, BoardColumnId = 4, Language = "en", Name = "Done" },
            new { Id = 8, BoardColumnId = 4, Language = "ru", Name = "Готово" },
            new { Id = 9, BoardColumnId = 4, Language = "az", Name = "Bitib" },
            // Done
            new { Id = 10, BoardColumnId = 3, Language = "en", Name = "Done" },
            new { Id = 11, BoardColumnId = 3, Language = "ru", Name = "Выполнено" },
            new { Id = 12, BoardColumnId = 3, Language = "az", Name = "Hazır" }
        );
    }
}
