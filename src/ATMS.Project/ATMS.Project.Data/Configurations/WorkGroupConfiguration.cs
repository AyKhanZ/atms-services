using ATMS.Data.Constants;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkGroupConfiguration : IEntityTypeConfiguration<WorkGroup>
{
    public void Configure(EntityTypeBuilder<WorkGroup> builder)
    {
        builder.HasIndex(e => new { e.WorkProjectId, e.ParentWorkGroupId, e.Title })
            .IsUnique();
            
            
        builder.HasIndex(e => e.Title)
            .IsUnique();
            
        builder.HasIndex(e => e.Code)
            .IsUnique();
            
            
        builder.Property(e => e.Title)
            .IsRequired();
            
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        builder.Property(e => e.Level)
            .IsRequired();
            
            
        builder.Property(u => u.StatusId)
            .HasDefaultValue(DefaultValues.DictionaryDefaultId)
            .IsRequired();
            
        builder.ToTable(t =>
            t.HasCheckConstraint("CK_WorkGroup_Level", "\"Level\" <= 1"));
            
            
        builder.HasOne(g => g.ParentWorkGroup)
            .WithMany(g => g.Children)
            .HasForeignKey(g => g.ParentWorkGroupId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(g => g.WorkTickets)
            .WithOne(t => t.WorkGroup)
            .HasForeignKey(t => t.WorkGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}