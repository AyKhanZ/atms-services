using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasIndex(e => new { e.UserId, e.CreatedAt, e.Id })
            .IsDescending(false, true, true);

        builder.HasIndex(e => new { e.CreatedAt, e.Id });

        // The Unread list walks only unread rows instead of skipping every read one on the way.
        builder.HasIndex(e => new { e.UserId, e.CreatedAt, e.Id }, "IX_Notifications_UserId_CreatedAt_Id_Unread")
            .IsDescending(false, true, true)
            .HasFilter("\"ReadAt\" IS NULL");

        // Finds the unread notification a new one merges into, and counts the unread for the bell:
        // both read only unread rows, a small part of the table once people read what they get.
        builder.HasIndex(e => new { e.UserId, e.Type, e.EntityId })
            .HasFilter("\"ReadAt\" IS NULL");

        // One deadline reminder per person per deadline, however many passes or instances run.
        builder.HasIndex(e => new { e.UserId, e.DedupKey })
            .IsUnique()
            .HasFilter("\"DedupKey\" IS NOT NULL");

        builder.Property(e => e.Type)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .IsRequired();

        builder.Property(e => e.DedupKey)
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.ComplexProperty(e => e.Parameters, parameters => parameters.ToJson());

        // No foreign key to the entity or the comment: they are soft-deleted, and a notification
        // outlives them to say so.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Actor)
            .WithMany()
            .HasForeignKey(e => e.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkProject>()
            .WithMany()
            .HasForeignKey(e => e.WorkProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
