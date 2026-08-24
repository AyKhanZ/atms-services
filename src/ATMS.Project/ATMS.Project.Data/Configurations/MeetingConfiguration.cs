using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.HasIndex(e => new { e.WorkProjectId, e.StartsAt });

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.Location)
            .HasMaxLength(300);

        builder.Property(e => e.MeetingUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasDefaultValue((int)MeetingStatusEnum.Planned)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedById)
            .IsRequired();

        builder.ConfigureSoftDeletableAuditUserRelationships<Meeting, User>();
    }
}
