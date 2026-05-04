using ATMS.Admin.Data.Entities.UserProgresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.UserProgresses;

public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.UserProgressType)
            .IsRequired();

        builder.Property(x => x.CurrentStep)
            .IsRequired()
            .HasDefaultValue((ushort)0);

        builder.Property(x => x.LastUpdated)
            .IsRequired();

        // 1:1 with User
        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<UserProgress>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:1 with PersonalInfo
        builder.HasOne(x => x.PersonalInfo)
            .WithOne(x => x.UserProgress)
            .HasForeignKey<PersonalInfo>(x => x.UserProgressId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // 1:N with InvitedUsers
        builder.HasMany(x => x.InvitedUsers)
            .WithOne(x => x.UserProgress)
            .HasForeignKey(x => x.UserProgressId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}