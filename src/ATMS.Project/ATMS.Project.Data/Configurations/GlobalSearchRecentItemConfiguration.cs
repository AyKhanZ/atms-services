using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class GlobalSearchRecentItemConfiguration : IEntityTypeConfiguration<GlobalSearchRecentItem>
{
    public void Configure(EntityTypeBuilder<GlobalSearchRecentItem> builder)
    {
        builder.ToTable("GlobalSearchRecentItems");

        builder.HasKey(item => new { item.UserId, item.ItemType, item.ItemId });

        builder.HasIndex(item => new { item.UserId, item.OpenedAt })
            .IsDescending(false, true);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
