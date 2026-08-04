using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Services;

public class EntityCodeGenerator(ProjectDbContext context) : IEntityCodeGenerator
{
    public async Task<string> GetNextAsync(CancellationToken cancellationToken)
    {
        var value = await context.Database
            .SqlQuery<long>($"SELECT nextval('\"EntityCodeSequence\"') AS \"Value\"")
            .SingleAsync(cancellationToken);

        return value.ToString();
    }
}
