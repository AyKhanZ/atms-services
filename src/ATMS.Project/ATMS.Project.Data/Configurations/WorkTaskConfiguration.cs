using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
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

        // The Tasks page searches "code or title" with ILIKE. Without this index the Title one is
        // useless for that query: an OR can use indexes only when both sides have one. A code is
        // written once at creation, so the index costs next to nothing on writes.
        builder.HasIndex(e => e.Code, "IX_Tasks_Code_Trigram")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => new { e.WorkTicketId, e.ParentWorkTaskId, e.CreatedAt, e.Id });

        builder.HasIndex(e => new { e.ParentWorkTaskId, e.CreatedAt, e.Id });

        builder.HasIndex(e => new { e.WorkProjectId, e.CreatedAt, e.Id });

        // One card per place in a New or In Progress column: two moves to the same spot at the same
        // moment would otherwise save the same key, and nothing could ever go between them again.
        // Done is ordered by close date, not by rank, and deleted cards hold no place.
        builder.HasIndex(e => new { e.StatusId, e.Rank }, UniqueRankIndex)
            .IsUnique()
            .HasFilter($"\"IsDeleted\" = false AND \"StatusId\" <> {(int)WorkTaskStatusEnum.Done}");

        builder.HasIndex(e => new { e.StatusId, e.DoneAt, e.Id });

        builder.HasIndex(e => new { e.Deadline, e.Id });


        builder.HasIndex(e => new { e.Rank, e.Id });

        // Plain ascending on both columns: the list sorts priority both ways with the id in the same
        // direction, and PostgreSQL reads one index backwards as well as forwards.
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
