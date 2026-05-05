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
    }
}