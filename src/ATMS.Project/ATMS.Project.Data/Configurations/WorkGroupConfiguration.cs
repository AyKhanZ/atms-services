using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkGroupConfiguration : IEntityTypeConfiguration<WorkGroup>
{
    public void Configure(EntityTypeBuilder<WorkGroup> builder)
    {
        builder.ToTable("ProjectGroups");

        builder.HasIndex(e => new { e.WorkProjectId, e.ParentWorkGroupId, e.Title })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("\"IsDeleted\" = false");
            
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        builder.Property(u => u.StatusId)
            .HasDefaultValue((int)WorkGroupStatusEnum.Planned)
            .IsRequired();
            
        builder.HasOne(g => g.ParentWorkGroup)
            .WithMany(g => g.Children)
            .HasForeignKey(g => g.ParentWorkGroupId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(g => g.WorkTickets)
            .WithOne(t => t.WorkGroup)
            .HasForeignKey(t => t.WorkGroupId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ConfigureSoftDeletableAuditUserRelationships<WorkGroup, User>();
    }
}
