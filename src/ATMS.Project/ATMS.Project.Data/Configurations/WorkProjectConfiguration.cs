using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkProjectConfiguration : IEntityTypeConfiguration<WorkProject>
{
    public void Configure(EntityTypeBuilder<WorkProject> builder)
    {
        builder.ToTable("Projects");

        builder.HasIndex(e => new { e.OrganizationId, e.Title })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("\"IsDeleted\" = false");
            
        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.CreatedAt);

        builder.HasIndex(e => e.StartDate);

        builder.HasIndex(e => e.EndDate);
        
            
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.CreatedById)
            .IsRequired();

            
        builder.Property(u => u.ProjectTypeId)
            .IsRequired();
            
        builder.Property(u => u.ProjectKindId)
            .IsRequired();

        builder.Property(u => u.ProjectStatusId)
            .IsRequired();

        builder.HasOne(p => p.Organization)
            .WithMany(o => o.WorkProjects)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UpdatedBy)
            .WithMany()
            .HasForeignKey(p => p.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
            
            
        builder.HasMany(p => p.WorkProjectParticipants)
            .WithOne(o => o.WorkProject)
            .HasForeignKey(o => o.WorkProjectId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasMany(p => p.WorkGroups)
            .WithOne(o => o.WorkProject)
            .HasForeignKey(o => o.WorkProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.Meetings)
            .WithOne(o => o.WorkProject)
            .HasForeignKey(o => o.WorkProjectId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
