using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations;

public class WorkProjectParticipantConfiguration : IEntityTypeConfiguration<WorkProjectParticipant>
{
    public void Configure(EntityTypeBuilder<WorkProjectParticipant> builder)
    {
        builder.HasIndex(e => new { e.WorkProjectId, e.UserId })
            .IsUnique();

            
        builder.HasOne(pp => pp.User)
            .WithMany()
            .HasForeignKey(pp => pp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pp => pp.WorkProjectParticipantRoles)
            .WithOne(ppr => ppr.WorkProjectParticipant)
            .HasForeignKey(ppr => ppr.WorkProjectParticipantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
