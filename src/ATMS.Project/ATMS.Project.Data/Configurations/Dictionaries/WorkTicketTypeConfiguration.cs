using ATMS.Data.Enums;
using ATMS.Project.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Dictionaries;

public class WorkTicketTypeConfiguration : IEntityTypeConfiguration<WorkTicketType>
{
    public void Configure(EntityTypeBuilder<WorkTicketType> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        
        builder.HasMany(p => p.Translations)
            .WithOne(t => t.WorkTicketType)
            .HasForeignKey(t => t.WorkTicketTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.HasData(
            new { Id = (int)WorkTicketTypeEnum.Bug, Code = "Bug" },
            new { Id = (int)WorkTicketTypeEnum.Feature, Code = "Feature" },
            new { Id = (int)WorkTicketTypeEnum.Task, Code = "Task" }
        );
    }
}

public class WorkTicketTypeTranslationConfiguration : IEntityTypeConfiguration<WorkTicketTypeTranslation>
{
    public void Configure(EntityTypeBuilder<WorkTicketTypeTranslation> builder)
    {
        builder.HasIndex(t => new { t.WorkTicketTypeId, t.Language })
            .IsUnique();

        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        
        builder.HasData(
            // Bug
            new { Id = 1, WorkTicketTypeId = (int)WorkTicketTypeEnum.Bug, Language = "en", Name = "Bug" },
            new { Id = 2, WorkTicketTypeId = (int)WorkTicketTypeEnum.Bug, Language = "ru", Name = "Ошибка" },
            new { Id = 3, WorkTicketTypeId = (int)WorkTicketTypeEnum.Bug, Language = "az", Name = "Xəta" },
            // Feature
            new { Id = 4, WorkTicketTypeId = (int)WorkTicketTypeEnum.Feature, Language = "en", Name = "Feature" },
            new { Id = 5, WorkTicketTypeId = (int)WorkTicketTypeEnum.Feature, Language = "ru", Name = "Новая функция" },
            new { Id = 6, WorkTicketTypeId = (int)WorkTicketTypeEnum.Feature, Language = "az", Name = "Təzə Funksiya" },
            // Task
            new { Id = 7, WorkTicketTypeId = (int)WorkTicketTypeEnum.Task, Language = "en", Name = "Task" },
            new { Id = 8, WorkTicketTypeId = (int)WorkTicketTypeEnum.Task, Language = "ru", Name = "Задача" },
            new { Id = 9, WorkTicketTypeId = (int)WorkTicketTypeEnum.Task, Language = "az", Name = "Tapşırıq" }
        );
    }
}
