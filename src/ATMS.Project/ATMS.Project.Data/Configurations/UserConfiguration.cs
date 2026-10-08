using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.Email)
            .IsUnique();
        
        // case-insensitive email lookup, so the same address isn't invited twice
        builder.HasIndex(e => e.NormalizedEmail);

        builder.HasIndex(e => e.UserType);

        builder.HasIndex(e => e.IsAdmin);

        builder.Property(e => e.Email)
            .HasMaxLength(256)
            .IsRequired();
            
        builder.Property(e => e.NormalizedEmail)
            .HasMaxLength(256)
            .IsRequired();
            
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(e => e.Surname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Position)
            .HasMaxLength(100);
        
        builder.Property(e => e.UserType)
            .IsRequired();

        builder.Property(e => e.IsAdmin)
            .IsRequired();
    }
}
