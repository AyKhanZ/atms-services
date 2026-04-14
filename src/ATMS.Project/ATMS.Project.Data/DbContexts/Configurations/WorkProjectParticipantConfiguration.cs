using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations;

public class WorkProjectParticipantConfiguration : IEntityTypeConfiguration<WorkProjectParticipant>
{
    public void Configure(EntityTypeBuilder<WorkProjectParticipant> builder)
    {
        throw new NotImplementedException();
    }
}
