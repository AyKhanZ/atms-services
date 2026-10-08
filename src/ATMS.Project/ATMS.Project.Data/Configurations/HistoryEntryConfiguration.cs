using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public sealed class HistoryEntryConfiguration : IEntityTypeConfiguration<HistoryEntry>
{
    public void Configure(EntityTypeBuilder<HistoryEntry> builder)
    {
        builder.ToTable("HistoryEntries");

        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.CreatedAt, e.Id })
            .IsDescending(false, false, true, true);

        builder.HasIndex(e => new { e.CreatedAt, e.Id })
            .IsDescending();

        // partial index: project history reads only project/group/milestone rows, a small part of the table
        builder.HasIndex(e => new { e.WorkProjectId, e.CreatedAt, e.Id })
            .IsDescending(false, true, true)
            .HasFilter("\"EntityType\" IN (1, 2, 3)");

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // no FK: EntityId points to a different table per type
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
