using MediatR;

namespace ATMS.Admin.Contracts.Requests.Me;

public class GetCurrentPermissionsRequest : IRequest<string[]>
{
    public Guid UserId { get; set; }
}
