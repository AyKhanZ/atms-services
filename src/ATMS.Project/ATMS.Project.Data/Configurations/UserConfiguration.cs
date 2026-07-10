using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.Email)
            .IsUnique();
        
        builder.HasIndex(e => e.UserType);

        builder.Property(e => e.Email)
            .HasMaxLength(256)
            .IsRequired();
            
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(e => e.Surname)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.UserType)
            .IsRequired();
    }
}
