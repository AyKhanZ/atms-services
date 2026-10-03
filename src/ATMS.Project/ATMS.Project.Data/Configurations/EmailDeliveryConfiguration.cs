using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class EmailDeliveryConfiguration : IEntityTypeConfiguration<EmailDelivery>
{
    public void Configure(EntityTypeBuilder<EmailDelivery> builder)
    {
        builder.ToTable("EmailDeliveries");

        // The sender reads only what still waits, oldest first; sent rows stay out of its way.
        builder.HasIndex(e => new { e.NextAttemptAt, e.CreatedAt })
            .HasFilter("\"Status\" = 1");

        builder.Property(e => e.LastError)
            .HasMaxLength(2000);

        // The email goes with its notification: cleanup that removes one removes the other.
        builder.HasOne(e => e.Notification)
            .WithMany()
            .HasForeignKey(e => e.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
