using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("Tasks", t =>
            t.HasCheckConstraint("CK_Tasks_Level", "\"Level\" <= 1"));

        builder.HasIndex(e => e.Code)
            .IsUnique();


        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.StatusId)
            .HasDefaultValue((int)WorkTaskStatusEnum.New)
            .IsRequired();

        builder.Property(e => e.PriorityId)
            .HasDefaultValue((int)WorkItemPriorityEnum.Low)
            .IsRequired();

        builder.Property(e => e.Level)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedById)
            .IsRequired();


        builder.HasOne(t => t.ParentWorkTask)
            .WithMany(t => t.Children)
            .HasForeignKey(t => t.ParentWorkTaskId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
