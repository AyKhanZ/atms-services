using ATMS.Admin.Data.Entities;
using ATMS.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.Email)
            .IsUnique();
        
        builder.Property(e => e.Email)
            .IsRequired();
        
        builder.Property(e => e.UserTypeId)
            .IsRequired();

        builder.HasIndex(e => e.RefreshToken)
            .IsUnique();

        
        builder.Property(e => e.AvatarPath)
            .HasDefaultValue(DefaultValues.UserAvatar);
        
        builder.Property(e => e.Language)
            .HasDefaultValue(DefaultValues.Language);

        
        builder.Property(u => u.MaritalStatusId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(u => u.UserStatusId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(u => u.GenderId)
            .HasDefaultValue(1)
            .IsRequired();
    }
}
