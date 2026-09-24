using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class HistoryEntryConfiguration : IEntityTypeConfiguration<HistoryEntry>
{
    public void Configure(EntityTypeBuilder<HistoryEntry> builder)
    {
        builder.ToTable("HistoryEntries");

        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.CreatedAt, e.Id })
            .IsDescending(false, false, true, true);

        builder.HasIndex(e => new { e.WorkProjectId, e.CreatedAt, e.Id })
            .IsDescending(false, true, true);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // No foreign key to the entity itself: EntityId points into a different table per type.
        builder.HasOne<WorkProject>()
            .WithMany()
            .HasForeignKey(e => e.WorkProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Changes)
            .WithOne(e => e.HistoryEntry)
            .HasForeignKey(e => e.HistoryEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
