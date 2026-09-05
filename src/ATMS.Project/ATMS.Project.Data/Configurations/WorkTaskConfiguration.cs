using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => new { e.WorkTicketId, e.ParentWorkTaskId, e.CreatedAt, e.Id });

        builder.HasIndex(e => new { e.ParentWorkTaskId, e.CreatedAt, e.Id });

        builder.HasIndex(e => new { e.WorkProjectId, e.CreatedAt, e.Id });


        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.StatusId)
            .HasDefaultValue((int)WorkTaskStatusEnum.New)
            .IsRequired();

        builder.Property(e => e.PriorityId)
            .HasDefaultValue((int)WorkItemPriorityEnum.Low)
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

        builder.ConfigureSoftDeletableAuditUserRelationships<WorkTask, User>();
    }
}
