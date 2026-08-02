using ATMS.Data.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations.Messaging;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.HasKey(x => new { x.MessageId, x.ConsumerName });

        builder.HasIndex(x => x.ProcessedAt);

        builder.Property(x => x.ConsumerName)
            .HasMaxLength(300)
            .IsRequired();
    }
}
