using ATMS.Admin.Data.Entities;
using ATMS.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.Property(e => e.Name)
            .IsRequired();

        builder.HasData(
            new { Id = RoleIds.ClientManager, Name = "Client Manager", Description = "Client Manager Role" },
            new { Id = RoleIds.Client, Name = "Client", Description = "Client Role" },
            new { Id = RoleIds.Agent, Name = "Agent", Description = "Agent Role" }
        );
    }
}
