using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations;

public class WorkGroupConfiguration : IEntityTypeConfiguration<WorkGroup>
{
    public void Configure(EntityTypeBuilder<WorkGroup> builder)
    {
        throw new NotImplementedException();
    }
}
