using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data.Enums;
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
            new { Id = (int)UserTypeEnum.Agent, Code = "Agent" },
            new { Id = (int)UserTypeEnum.Client, Code = "Client" },
            new { Id = (int)UserTypeEnum.ClientManager, Code = "ClientManager" }
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
            new { Id = 1, UserTypeId = (int)UserTypeEnum.Agent, Language = "en", Name = "Agent" },
            new { Id = 2, UserTypeId = (int)UserTypeEnum.Agent, Language = "ru", Name = "Агент" },
            new { Id = 3, UserTypeId = (int)UserTypeEnum.Agent, Language = "az", Name = "Agent" },
            // Client
            new { Id = 4, UserTypeId = (int)UserTypeEnum.Client, Language = "en", Name = "Client" },
            new { Id = 5, UserTypeId = (int)UserTypeEnum.Client, Language = "ru", Name = "Клиент" },
            new { Id = 6, UserTypeId = (int)UserTypeEnum.Client, Language = "az", Name = "Müştəri" },
            // Client Manager
            new { Id = 7, UserTypeId = (int)UserTypeEnum.ClientManager, Language = "en", Name = "Client Manager" },
            new { Id = 8, UserTypeId = (int)UserTypeEnum.ClientManager, Language = "ru", Name = "Менеджер клиентов" },
            new { Id = 9, UserTypeId = (int)UserTypeEnum.ClientManager, Language = "az", Name = "Müştəri meneceri" }
        );
    }
}
