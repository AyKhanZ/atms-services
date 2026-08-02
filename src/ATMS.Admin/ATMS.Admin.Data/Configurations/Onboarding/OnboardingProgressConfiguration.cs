using ATMS.Admin.Data.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Onboarding;

public class OnboardingProgressConfiguration : IEntityTypeConfiguration<OnboardingProgress>
{
    public void Configure(EntityTypeBuilder<OnboardingProgress> builder)
    {
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.PersonalInfoStatus)
            .IsRequired();

        builder.Property(x => x.SecurityStatus)
            .IsRequired();

        builder.Property(x => x.InvitationsStatus)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<OnboardingProgress>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
