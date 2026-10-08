using ATMS.Admin.Contracts.Requests.Health;
using ATMS.Admin.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Health;

public sealed class CheckReadinessHandler(IHealthRepository healthRepository) : IRequestHandler<CheckReadinessRequest, bool>
{
    public Task<bool> Handle(CheckReadinessRequest request, CancellationToken cancellationToken)
    {
        return healthRepository.IsReadyAsync(cancellationToken);
    }
}
