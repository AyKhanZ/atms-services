using MediatR;

namespace ATMS.Admin.Contracts.Commands.Role;

public class DeleteRoleCommand : IRequest
{
    public required Guid Id { get; init; }
}
