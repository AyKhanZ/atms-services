using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public const string UniqueRankIndex = "IX_Tasks_StatusId_Rank";

    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasIndex(task => task.Title)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.Code)
            .IsUnique();

        // trigram index for the "code or title" ILIKE search, an OR uses indexes only if both sides have one
        builder.HasIndex(e => e.Code, "IX_Tasks_Code_Trigram")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => new { e.WorkTicketId, e.ParentWorkTaskId, e.CreatedAt, e.Id });

        builder.HasIndex(e => new { e.ParentWorkTaskId, e.CreatedAt, e.Id });

        builder.HasIndex(e => new { e.WorkProjectId, e.CreatedAt, e.Id });

        // one card per place in New / In Progress, two moves to the same spot can't save the same rank
        builder.HasIndex(e => new { e.StatusId, e.Rank }, UniqueRankIndex)
            .IsUnique()
            .HasFilter($"\"IsDeleted\" = false AND \"StatusId\" <> {(int)WorkTaskStatusEnum.Done}");

        builder.HasIndex(e => new { e.StatusId, e.DoneAt, e.Id });

        builder.HasIndex(e => new { e.Deadline, e.Id });


        builder.HasIndex(e => new { e.Rank, e.Id });

        // postgres reads an index both ways, so plain ascending works for both sort directions
        builder.HasIndex(e => new { e.PriorityId, e.Id });


        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Rank)
            .IsRequired()
            .HasMaxLength(64)
            .UseCollation("C");

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
