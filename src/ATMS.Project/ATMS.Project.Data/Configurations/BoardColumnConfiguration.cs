using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public sealed class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.ToTable("BoardColumns");

        builder.Property(column => column.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(column => column.Code)
            .IsUnique();

        builder.HasIndex(column => column.Order)
            .IsUnique();

        builder.HasMany(column => column.Translations)
            .WithOne(translation => translation.BoardColumn)
            .HasForeignKey(translation => translation.BoardColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-3 keep the ids of their statuses: existing tasks are filled from StatusId, and a status change
        // outside the board lands there. In Progress is split into Doing and Done, like split columns in Azure DevOps
        builder.HasData(
            new { Id = 1, Code = "New", Category = (int)WorkTaskStatusEnum.New, Order = 1 },
            new { Id = 2, Code = "InProgressDoing", Category = (int)WorkTaskStatusEnum.InProgress, Order = 2 },
            new { Id = 4, Code = "InProgressDone", Category = (int)WorkTaskStatusEnum.InProgress, Order = 3 },
            new { Id = 3, Code = "Done", Category = (int)WorkTaskStatusEnum.Done, Order = 4 }
        );
    }
}
