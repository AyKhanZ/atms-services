using MediatR;

namespace ATMS.Project.Contracts.Requests.Health;

public sealed class CheckReadinessRequest : IRequest<bool>;
