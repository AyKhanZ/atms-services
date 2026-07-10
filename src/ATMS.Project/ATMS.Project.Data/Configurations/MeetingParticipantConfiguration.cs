using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class MeetingParticipantConfiguration : IEntityTypeConfiguration<MeetingParticipant>
{
    public void Configure(EntityTypeBuilder<MeetingParticipant> builder)
    {
        builder.ToTable("MeetingParticipants");

        builder.HasIndex(e => new { e.MeetingId, e.ParticipantId })
            .IsUnique();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .HasDefaultValue(MeetingParticipantStatusEnum.Pending)
            .IsRequired();

        builder.HasOne(e => e.Meeting)
            .WithMany(e => e.Participants)
            .HasForeignKey(e => e.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Participant)
            .WithMany()
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
