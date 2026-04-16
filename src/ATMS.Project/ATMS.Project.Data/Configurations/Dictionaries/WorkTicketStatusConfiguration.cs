using ATMS.Data.Enums;
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
            new { Id = (int)WorkTicketStatusEnum.New, Code = "New" },
            new { Id = (int)WorkTicketStatusEnum.InProgress, Code = "InProgress" },
            new { Id = (int)WorkTicketStatusEnum.InReview, Code = "InReview" },
            new { Id = (int)WorkTicketStatusEnum.Testing, Code = "Testing" },
            new { Id = (int)WorkTicketStatusEnum.Closed, Code = "Closed" },
            new { Id = (int)WorkTicketStatusEnum.Rejected, Code = "Rejected" }
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
            new { Id = 1, WorkTicketStatusId = (int)WorkTicketStatusEnum.New, Language = "en", Name = "New" },
            new { Id = 2, WorkTicketStatusId = (int)WorkTicketStatusEnum.New, Language = "ru", Name = "Новый" },
            new { Id = 3, WorkTicketStatusId = (int)WorkTicketStatusEnum.New, Language = "az", Name = "Yeni" },
            // InProgress
            new { Id = 4, WorkTicketStatusId = (int)WorkTicketStatusEnum.InProgress, Language = "en", Name = "In Progress" },
            new { Id = 5, WorkTicketStatusId = (int)WorkTicketStatusEnum.InProgress, Language = "ru", Name = "В работе" },
            new { Id = 6, WorkTicketStatusId = (int)WorkTicketStatusEnum.InProgress, Language = "az", Name = "İşdə" },
            // InReview
            new { Id = 7, WorkTicketStatusId = (int)WorkTicketStatusEnum.InReview, Language = "en", Name = "In Review" },
            new { Id = 8, WorkTicketStatusId = (int)WorkTicketStatusEnum.InReview, Language = "ru", Name = "На проверке" },
            new { Id = 9, WorkTicketStatusId = (int)WorkTicketStatusEnum.InReview, Language = "az", Name = "Yoxlamada" },
            // Testing
            new { Id = 10, WorkTicketStatusId = (int)WorkTicketStatusEnum.Testing, Language = "en", Name = "Testing" },
            new { Id = 11, WorkTicketStatusId = (int)WorkTicketStatusEnum.Testing, Language = "ru", Name = "Тестирование" },
            new { Id = 12, WorkTicketStatusId = (int)WorkTicketStatusEnum.Testing, Language = "az", Name = "Test mərhələsində" },
            // Closed
            new { Id = 13, WorkTicketStatusId = (int)WorkTicketStatusEnum.Closed, Language = "en", Name = "Closed" },
            new { Id = 14, WorkTicketStatusId = (int)WorkTicketStatusEnum.Closed, Language = "ru", Name = "Закрыт" },
            new { Id = 15, WorkTicketStatusId = (int)WorkTicketStatusEnum.Closed, Language = "az", Name = "Bağlandı" },
            // Rejected
            new { Id = 16, WorkTicketStatusId = (int)WorkTicketStatusEnum.Rejected, Language = "en", Name = "Rejected" },
            new { Id = 17, WorkTicketStatusId = (int)WorkTicketStatusEnum.Rejected, Language = "ru", Name = "Отклонён" },
            new { Id = 18, WorkTicketStatusId = (int)WorkTicketStatusEnum.Rejected, Language = "az", Name = "Rədd edildi" }
        );
    }
}