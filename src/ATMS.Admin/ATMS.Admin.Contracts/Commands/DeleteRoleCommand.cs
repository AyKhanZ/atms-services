using MediatR;

namespace ATMS.Admin.Contracts.Commands;

public class DeleteRoleCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
