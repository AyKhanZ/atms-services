using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkProjectInvitationConfiguration : IEntityTypeConfiguration<WorkProjectInvitation>
{
    public void Configure(EntityTypeBuilder<WorkProjectInvitation> builder)
    {
        builder.ToTable("ProjectInvitations");

        // Not unique: an invitation nobody answered for 24 hours stays pending in the table, and the
        // same email may be invited again. The validator keeps live duplicates out.
        builder.HasIndex(e => new { e.WorkProjectId, e.NormalizedEmail })
            .HasFilter("\"Status\" = 1");

        builder.HasIndex(e => e.NormalizedEmail)
            .HasFilter("\"Status\" = 1");

        builder.Property(e => e.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.NormalizedEmail)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Surname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasOne(e => e.WorkProject)
            .WithMany()
            .HasForeignKey(e => e.WorkProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InvitedBy)
            .WithMany()
            .HasForeignKey(e => e.InvitedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
