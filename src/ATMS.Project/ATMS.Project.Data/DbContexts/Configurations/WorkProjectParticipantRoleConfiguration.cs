using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Project.Data.DbContexts.Configurations;

public class WorkProjectParticipantRoleConfiguration : IEntityTypeConfiguration<WorkProjectParticipantRole>
{
    public void Configure(EntityTypeBuilder<WorkProjectParticipantRole> builder)
    {
        throw new NotImplementedException();
    }
}