using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class WorkTicketStatusConfiguration : IEntityTypeConfiguration<WorkTicketStatus>
{
    public void Configure(EntityTypeBuilder<WorkTicketStatus> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(p => p.Translations)
            .WithOne(t => t.WorkTicketStatus)
            .HasForeignKey(t => t.WorkTicketStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new { Id = 1, Code = "New" },
            new { Id = 2, Code = "InProgress" },
            new { Id = 3, Code = "InReview" },
            new { Id = 4, Code = "Testing" },
            new { Id = 5, Code = "Closed" },
            new { Id = 6, Code = "Rejected" }
        );
    }
}

public class WorkTicketStatusTranslationConfiguration : IEntityTypeConfiguration<WorkTicketStatusTranslation>
{
    public void Configure(EntityTypeBuilder<WorkTicketStatusTranslation> builder)
    {
        builder.HasIndex(t => new { t.WorkTicketStatusId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.HasData(
            // New
            new { Id = 1, WorkTicketStatusId = 1, Language = "en", Name = "New" },
            new { Id = 2, WorkTicketStatusId = 1, Language = "ru", Name = "Новый" },
            new { Id = 3, WorkTicketStatusId = 1, Language = "az", Name = "Yeni" },
            // InProgress
            new { Id = 4, WorkTicketStatusId = 2, Language = "en", Name = "In Progress" },
            new { Id = 5, WorkTicketStatusId = 2, Language = "ru", Name = "В работе" },
            new { Id = 6, WorkTicketStatusId = 2, Language = "az", Name = "İşdə" },
            // InReview
            new { Id = 7, WorkTicketStatusId = 3, Language = "en", Name = "In Review" },
            new { Id = 8, WorkTicketStatusId = 3, Language = "ru", Name = "На проверке" },
            new { Id = 9, WorkTicketStatusId = 3, Language = "az", Name = "Yoxlamada" },
            // Testing
            new { Id = 10, WorkTicketStatusId = 4, Language = "en", Name = "Testing" },
            new { Id = 11, WorkTicketStatusId = 4, Language = "ru", Name = "Тестирование" },
            new { Id = 12, WorkTicketStatusId = 4, Language = "az", Name = "Test mərhələsində" },
            // Closed
            new { Id = 13, WorkTicketStatusId = 5, Language = "en", Name = "Closed" },
            new { Id = 14, WorkTicketStatusId = 5, Language = "ru", Name = "Закрыт" },
            new { Id = 15, WorkTicketStatusId = 5, Language = "az", Name = "Bağlandı" },
            // Rejected
            new { Id = 16, WorkTicketStatusId = 6, Language = "en", Name = "Rejected" },
            new { Id = 17, WorkTicketStatusId = 6, Language = "ru", Name = "Отклонён" },
            new { Id = 18, WorkTicketStatusId = 6, Language = "az", Name = "Rədd edildi" }
        );
    }
}