using ATMS.Admin.Data.Entities.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Tokens;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasIndex(e => e.UserId);

        builder.HasIndex(e => e.TokenHash)
            .IsUnique();

        builder.Property(e => e.TokenHash)
            .HasMaxLength(64)
            .IsRequired();
    }
}
