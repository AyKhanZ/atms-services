using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.Configurations;

public class HistoryChangeConfiguration : IEntityTypeConfiguration<HistoryChange>
{
    public void Configure(EntityTypeBuilder<HistoryChange> builder)
    {
        builder.ToTable("HistoryChanges");
    }
}
