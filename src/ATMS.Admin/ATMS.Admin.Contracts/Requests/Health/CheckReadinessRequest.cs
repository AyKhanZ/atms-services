using MediatR;

namespace ATMS.Admin.Contracts.Requests.Health;

public class CheckReadinessRequest : IRequest<bool>;
