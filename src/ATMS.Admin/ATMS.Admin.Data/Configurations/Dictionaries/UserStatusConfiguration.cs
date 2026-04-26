using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Dictionaries;

public class UserStatusConfiguration : IEntityTypeConfiguration<UserStatus>
{
    public void Configure(EntityTypeBuilder<UserStatus> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();
        
        
        builder.HasMany(u => u.Translations)
            .WithOne(t => t.UserStatus)
            .HasForeignKey(t => t.UserStatusId)
            .OnDelete(DeleteBehavior.Cascade);
        

        builder.HasData(
            new { Id = (int)UserStatusEnum.Active, Code = "Active" },
            new { Id = (int)UserStatusEnum.Inactive, Code = "Inactive" },
            new { Id = (int)UserStatusEnum.Locked, Code = "Locked" }
        );
    }
}

public class UserStatusTranslationConfiguration : IEntityTypeConfiguration<UserStatusTranslation>
{
    public void Configure(EntityTypeBuilder<UserStatusTranslation> builder)
    {
        builder.HasIndex(t => new { t.UserStatusId, t.Language })
            .IsUnique();
        
        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();
        
        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();
        

        builder.HasData(
            // Active
            new { Id = 1, UserStatusId = (int)UserStatusEnum.Active, Language = "en", Name = "Active" },
            new { Id = 2, UserStatusId = (int)UserStatusEnum.Active, Language = "ru", Name = "Активный" },
            new { Id = 3, UserStatusId = (int)UserStatusEnum.Active, Language = "az", Name = "Aktiv" },
            // Inactive
            new { Id = 4, UserStatusId = (int)UserStatusEnum.Inactive, Language = "en", Name = "Inactive" },
            new { Id = 5, UserStatusId = (int)UserStatusEnum.Inactive, Language = "ru", Name = "Неактивный" },
            new { Id = 6, UserStatusId = (int)UserStatusEnum.Inactive, Language = "az", Name = "Qeyri-aktiv" },
            // Locked
            new { Id = 7, UserStatusId = (int)UserStatusEnum.Locked, Language = "en", Name = "Locked" },
            new { Id = 8, UserStatusId = (int)UserStatusEnum.Locked, Language = "ru", Name = "Заблокирован" },
            new { Id = 9, UserStatusId = (int)UserStatusEnum.Locked, Language = "az", Name = "Bloklanmış" }
        );
    }
}
