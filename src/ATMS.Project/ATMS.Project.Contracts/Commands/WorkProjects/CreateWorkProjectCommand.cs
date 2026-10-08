using MediatR;
using ATMS.Application.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[SuperAdminAccess]
public sealed class CreateWorkProjectCommand : WorkProjectCommand, IRequest<Guid>;
