using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations;

public class WorkTicketConfiguration : IEntityTypeConfiguration<WorkTicket>
{
    public void Configure(EntityTypeBuilder<WorkTicket> builder)
    {
        throw new NotImplementedException();
    }
}
