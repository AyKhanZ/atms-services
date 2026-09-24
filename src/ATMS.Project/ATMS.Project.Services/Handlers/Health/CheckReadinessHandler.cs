using ATMS.Infrastructure.Files;
using ATMS.Project.Contracts.Requests.Health;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Health;

public class CheckReadinessHandler(
    IHealthRepository healthRepository,
    IFileStorage fileStorage) : IRequestHandler<CheckReadinessRequest, bool>
{
    public async Task<bool> Handle(CheckReadinessRequest request, CancellationToken cancellationToken)
    {
        return await healthRepository.IsReadyAsync(cancellationToken) &&
               await fileStorage.IsWritableAsync(cancellationToken);
    }
}
