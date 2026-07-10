using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkProjectParticipantRoleConfiguration : IEntityTypeConfiguration<WorkProjectParticipantRole>
{
    public void Configure(EntityTypeBuilder<WorkProjectParticipantRole> builder)
    {
        builder.ToTable("ProjectParticipantRoles");

        builder.HasIndex(e => new { e.WorkProjectParticipantId, e.RoleId })
            .IsUnique();
            

        builder.HasOne(ppr => ppr.Role)
            .WithMany(r => r.WorkProjectParticipantRoles)
            .HasForeignKey(ppr => ppr.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
