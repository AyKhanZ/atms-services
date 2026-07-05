using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class MeetingAgendaItemConfiguration : IEntityTypeConfiguration<MeetingAgendaItem>
{
    public void Configure(EntityTypeBuilder<MeetingAgendaItem> builder)
    {
        builder.ToTable("MeetingAgendaItems");

        builder.HasIndex(e => new { e.MeetingId, e.Order })
            .IsUnique();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(e => e.Meeting)
            .WithMany(e => e.AgendaItems)
            .HasForeignKey(e => e.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
