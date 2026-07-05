using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class WorkTicketConfiguration : IEntityTypeConfiguration<WorkTicket>
{
    public void Configure(EntityTypeBuilder<WorkTicket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasIndex(e => e.Code)
            .IsUnique();


        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Title)
            .IsRequired();

        builder.Property(e => e.WorkTicketStatusId)
            .HasDefaultValue((int)WorkTicketStatusEnum.New)
            .IsRequired();

        builder.Property(e => e.WorkTicketTypeId)
            .IsRequired();

        builder.Property(e => e.PriorityId)
            .HasDefaultValue((int)WorkItemPriorityEnum.Low)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedById)
            .IsRequired();


        builder.HasOne(t => t.WorkGroup)
            .WithMany(g => g.WorkTickets)
            .HasForeignKey(t => t.WorkGroupId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(t => t.WorkTasks)
            .WithOne(wt => wt.WorkTicket)
            .HasForeignKey(wt => wt.WorkTicketId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(t => t.Meetings)
            .WithOne(m => m.WorkTicket)
            .HasForeignKey(m => m.WorkTicketId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
