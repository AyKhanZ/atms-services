using MediatR;

namespace ATMS.Admin.Contracts.Requests.Me;

public sealed class GetCurrentPermissionsRequest : IRequest<string[]>;
