using MediatR;

namespace ATMS.Admin.Contracts.Commands.Role;

public class DeleteRoleCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
