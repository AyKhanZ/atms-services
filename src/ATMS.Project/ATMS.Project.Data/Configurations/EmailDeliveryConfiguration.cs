using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public sealed class EmailDeliveryConfiguration : IEntityTypeConfiguration<EmailDelivery>
{
    public void Configure(EntityTypeBuilder<EmailDelivery> builder)
    {
        builder.ToTable("EmailDeliveries");

        // the sender reads only pending rows, oldest first
        builder.HasIndex(e => new { e.NextAttemptAt, e.CreatedAt })
            .HasFilter("\"Status\" = 1");

        builder.Property(e => e.LastError)
            .HasMaxLength(2000);

        builder.HasOne(e => e.Notification)
            .WithMany()
            .HasForeignKey(e => e.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
