using ATMS.Admin.Data.Entities;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(e => e.Name)
            .IsUnique();
        
        builder.HasIndex(e => e.UserType);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.Description)
            .HasMaxLength(200);
        
        builder.Property(e => e.UserType)
            .IsRequired();
        
        builder.Property(e => e.IsSystem)
            .IsRequired();

        builder.HasData(
            new
            {
                Id = RoleIds.ClientManager,
                Name = "Client Manager",
                Description = "Client Manager Role",
                IsSystem = true,
                UserType = (int)UserTypeEnum.Client
            },
            new
            {
                Id = RoleIds.Client,
                Name = "Client",
                Description = "Client Role",
                IsSystem = true,
                UserType = (int)UserTypeEnum.Client
            },
            new
            {
                Id = RoleIds.Agent,
                Name = "Agent",
                Description = "Agent Role",
                IsSystem = true,
                UserType = (int)UserTypeEnum.Agent
            }
        );
    }
}