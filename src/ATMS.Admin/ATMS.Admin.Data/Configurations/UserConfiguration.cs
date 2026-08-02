using ATMS.Admin.Data.Entities;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.HasIndex(e => e.NormalizedEmail)
            .IsUnique();

        builder.HasIndex(e => e.RefreshToken)
            .IsUnique();
        
        builder.HasIndex(u => u.UserStatusId);
        
        builder.HasIndex(u => u.CreatedAt);
        
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.Surname)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.Email)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.AvatarPath)
            .HasDefaultValue(DefaultValues.UserAvatar);
        
        builder.Property(e => e.NormalizedEmail)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LanguageId)
            .HasDefaultValue(DefaultValues.Language)
            .IsRequired();

        builder.HasOne(e => e.Language)
            .WithMany()
            .HasForeignKey(e => e.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        
        builder.Property(u => u.IsAdmin)
            .HasDefaultValue(false)
            .IsRequired();
        
        builder.Property(u => u.MaritalStatusId)
            .HasDefaultValue((int)MaritalStatusEnum.NotSpecified)
            .IsRequired();

        builder.Property(u => u.UserStatusId)
            .HasDefaultValue((int)UserStatusEnum.Active)
            .IsRequired();

        builder.Property(u => u.GenderId)
            .HasDefaultValue((int)GenderEnum.NotSpecified)
            .IsRequired();
    }
}
