using ATMS.Data.Enums;
using ATMS.Data.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Messaging;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.Status, x.NextAttemptAt });

        builder.Property(x => x.Exchange)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.RoutingKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.MessageType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.NextAttemptAt)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);
    }
}
