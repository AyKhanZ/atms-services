using ATMS.Admin.Data.Entities.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Tokens;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasIndex(session => session.TokenHash)
            .IsUnique();

        builder.HasIndex(session => session.UserId);
        builder.HasIndex(session => session.FamilyId);
        builder.HasIndex(session => session.FamilyExpiresAt);

        builder.Property(session => session.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(session => session.RevokedAt)
            .IsConcurrencyToken();

        builder.HasOne(session => session.User)
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
