using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasIndex(e => new { e.UserId, e.CreatedAt, e.Id })
            .IsDescending(false, true, true);

        builder.HasIndex(e => new { e.CreatedAt, e.Id });

        // unread only, so the Unread list doesn't walk past all the read rows
        builder.HasIndex(e => new { e.UserId, e.CreatedAt, e.Id }, "IX_Notifications_UserId_CreatedAt_Id_Unread")
            .IsDescending(false, true, true)
            .HasFilter("\"ReadAt\" IS NULL");

        // finds the unread one to merge into and counts the bell, both read only unread rows
        builder.HasIndex(e => new { e.UserId, e.Type, e.EntityId })
            .HasFilter("\"ReadAt\" IS NULL");

        // one deadline reminder per person per deadline
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

        // no FK: the entity and the comment are soft-deleted, the notification lives longer
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
