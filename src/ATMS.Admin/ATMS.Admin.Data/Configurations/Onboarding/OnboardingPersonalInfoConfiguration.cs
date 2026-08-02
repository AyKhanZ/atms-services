using ATMS.Admin.Data.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Onboarding;

public class OnboardingPersonalInfoConfiguration : IEntityTypeConfiguration<OnboardingPersonalInfo>
{
    public void Configure(EntityTypeBuilder<OnboardingPersonalInfo> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Surname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Position)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AvatarPath)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.BirthDate)
            .IsRequired();

        builder.HasOne(x => x.Progress)
            .WithOne(x => x.PersonalInfo)
            .HasForeignKey<OnboardingPersonalInfo>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Gender)
            .WithMany()
            .HasForeignKey(x => x.GenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MaritalStatus)
            .WithMany()
            .HasForeignKey(x => x.MaritalStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
