using MediatR;

namespace ATMS.Admin.Contracts.Requests.Health;

public sealed class CheckReadinessRequest : IRequest<bool>;
