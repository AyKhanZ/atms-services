using MediatR;

namespace ATMS.Admin.Contracts.Commands.Role;

public class CreateRoleCommand : RoleCommand, IRequest<Guid>;
