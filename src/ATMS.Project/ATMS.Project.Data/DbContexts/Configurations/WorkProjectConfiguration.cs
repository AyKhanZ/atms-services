using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations;

public class WorkProjectConfiguration : IEntityTypeConfiguration<WorkProject>
{
    public void Configure(EntityTypeBuilder<WorkProject> builder)
    {
        builder.HasIndex(e => new { e.OrganizationId, e.Title })
            .IsUnique();
            
        builder.HasIndex(e => e.Title)
            .IsUnique();
            
        builder.HasIndex(e => e.Code)
            .IsUnique();
        
            
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.CreatedById)
            .IsRequired();

            
        builder.Property(u => u.ProjectTypeId)
            .HasDefaultValue(1)
            .IsRequired();
            
        builder.Property(u => u.ProjectKindId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(u => u.ProjectStatusId)
            .HasDefaultValue(1)
            .IsRequired();
            
            
        builder.HasMany(p => p.WorkProjectParticipants)
            .WithOne(o => o.WorkProject)
            .HasForeignKey(o => o.WorkProjectId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(p => p.WorkGroups)
            .WithOne(o => o.WorkProject)
            .HasForeignKey(o => o.WorkProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
