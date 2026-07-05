using MediatR;

namespace ATMS.Project.Contracts.Requests.Health;

public class CheckReadinessRequest : IRequest<bool>;
