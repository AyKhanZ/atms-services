using ATMS.Admin.Data.Entities.Messaging;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Messaging;

public class EmailDeliveryConfiguration : IEntityTypeConfiguration<EmailDelivery>
{
    public void Configure(EntityTypeBuilder<EmailDelivery> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.Status, x.NextAttemptAt });

        builder.HasIndex(x => new { x.UserId, x.Type, x.Status });

        builder.Property(x => x.TemporaryPassword)
            .HasMaxLength(40);

        builder.Property(x => x.PasswordResetToken)
            .HasMaxLength(2000);

        // Name, space and surname of the user who invited. A super administrator registers names of up
        // to 100, so the longest is 100 + 1 + 100.
        builder.Property(x => x.InviterName)
            .HasMaxLength(201);

        builder.Property(x => x.ProjectTitle)
            .HasMaxLength(80);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.NextAttemptAt)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
