using ATMS.Admin.Data.Entities.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Dictionaries;

public class UserTypeConfiguration : IEntityTypeConfiguration<UserType>
{
    public void Configure(EntityTypeBuilder<UserType> builder)
    {
        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        
        builder.HasMany(u => u.Translations)
            .WithOne(t => t.UserType)
            .HasForeignKey(t => t.UserTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.HasData(
            new { Id = 1, Code = "Agent" },
            new { Id = 2, Code = "Client" }
        );
    }
}

public class UserTypeTranslationConfiguration : IEntityTypeConfiguration<UserTypeTranslation>
{
    public void Configure(EntityTypeBuilder<UserTypeTranslation> builder)
    {
        builder.HasIndex(t => new { t.UserTypeId, t.Language })
            .IsUnique();
        
        builder.Property(t => t.Language)
            .HasMaxLength(2)
            .IsRequired();
        
        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();
        

        builder.HasData(
            // Agent
            new { UserTypeId = 1, Language = "en", Name = "Agent" },
            new { UserTypeId = 1, Language = "ru", Name = "Агент" },
            new { UserTypeId = 1, Language = "az", Name = "Agent" },
            // Client
            new { UserTypeId = 2, Language = "en", Name = "Client" },
            new { UserTypeId = 2, Language = "ru", Name = "Клиент" },
            new { UserTypeId = 2, Language = "az", Name = "Müştəri" }
        );
    }
}
