using ATMS.Project.Contracts.Requests.Health;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Health;

public class CheckReadinessHandler(IHealthRepository healthRepository) : IRequestHandler<CheckReadinessRequest, bool>
{
    public Task<bool> Handle(CheckReadinessRequest request, CancellationToken cancellationToken)
    {
        return healthRepository.IsReadyAsync(cancellationToken);
    }
}
