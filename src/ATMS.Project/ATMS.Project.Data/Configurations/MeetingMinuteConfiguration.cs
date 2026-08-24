using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class MeetingMinuteConfiguration : IEntityTypeConfiguration<MeetingMinute>
{
    public void Configure(EntityTypeBuilder<MeetingMinute> builder)
    {
        builder.HasIndex(e => new { e.MeetingId, e.Order })
            .IsUnique();

        builder.Property(e => e.Text)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedById)
            .IsRequired();

        builder.HasOne(e => e.Meeting)
            .WithMany(e => e.Minutes)
            .HasForeignKey(e => e.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditUserRelationships<MeetingMinute, User>();
    }
}
