using ATMS.Admin.Data.Entities.UserProgresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.UserProgresses;

public class InvitedUserConfiguration : IEntityTypeConfiguration<InvitedUser>
{
    public void Configure(EntityTypeBuilder<InvitedUser> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => x.Email)
            .IsUnique();

        
        builder.Property(x => x.UserProgressId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Surname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();
    }
}