using ATMS.Admin.Data.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Onboarding;

public class OnboardingInvitedUserConfiguration : IEntityTypeConfiguration<OnboardingInvitedUser>
{
    public void Configure(EntityTypeBuilder<OnboardingInvitedUser> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique();

        builder.HasIndex(x => x.OnboardingUserId);

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Surname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(x => x.Progress)
            .WithMany(x => x.InvitedUsers)
            .HasForeignKey(x => x.OnboardingUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
