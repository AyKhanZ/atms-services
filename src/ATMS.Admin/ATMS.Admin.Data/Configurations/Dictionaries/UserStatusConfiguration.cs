using ATMS.Admin.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Dictionaries;

public class UserStatusConfiguration : IEntityTypeConfiguration<UserStatus>
{
    public void Configure(EntityTypeBuilder<UserStatus> builder)
    {
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();
        
        
        builder.HasMany(u => u.Translations)
            .WithOne(t => t.UserStatus)
            .HasForeignKey(t => t.UserStatusId)
            .OnDelete(DeleteBehavior.Cascade);
        

        builder.HasData(
            new { Id = 1, Code = "Active" },
            new { Id = 2, Code = "Inactive" },
            new { Id = 3, Code = "Locked" }
        );
    }
}

public class UserStatusTranslationConfiguration : IEntityTypeConfiguration<UserStatusTranslation>
{
    public void Configure(EntityTypeBuilder<UserStatusTranslation> builder)
    {
        builder.HasIndex(t => new { t.UserStatusId, t.Language }).IsUnique();
        
        builder.Property(t => t.Language).HasMaxLength(5).IsRequired();
        
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        

        builder.HasData(
            new { UserStatusId = 1, Language = "en", Name = "Active" },
            new { UserStatusId = 1, Language = "ru", Name = "Активный" },
            new { UserStatusId = 1, Language = "az", Name = "Aktiv" },
            new { UserStatusId = 2, Language = "en", Name = "Inactive" },
            new { UserStatusId = 2, Language = "ru", Name = "Неактивный" },
            new { UserStatusId = 2, Language = "az", Name = "Qeyri-aktiv" },
            new { UserStatusId = 3, Language = "en", Name = "Locked" },
            new { UserStatusId = 3, Language = "ru", Name = "Заблокирован" },
            new { UserStatusId = 3, Language = "az", Name = "Bloklanmış" }
        );
    }
}
