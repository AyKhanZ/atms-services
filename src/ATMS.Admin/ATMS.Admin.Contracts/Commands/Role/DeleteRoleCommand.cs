using MediatR;

namespace ATMS.Admin.Contracts.Commands.Role;

public class DeleteRoleCommand : IRequest
{
    public Guid Id { get; set; }
}
