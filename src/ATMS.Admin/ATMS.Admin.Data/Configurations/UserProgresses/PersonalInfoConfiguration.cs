using ATMS.Admin.Data.Entities.UserProgresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.UserProgresses;

public class PersonalInfoConfiguration : IEntityTypeConfiguration<PersonalInfo>
{
    public void Configure(EntityTypeBuilder<PersonalInfo> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserProgressId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
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

        builder.Property(x => x.Language)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.AvatarPath)
            .IsRequired();

        builder.Property(x => x.BirthDate)
            .IsRequired();

        builder.Property(x => x.GenderId)
            .IsRequired();

        builder.Property(x => x.MaritalStatusId)
            .IsRequired();
    }
}