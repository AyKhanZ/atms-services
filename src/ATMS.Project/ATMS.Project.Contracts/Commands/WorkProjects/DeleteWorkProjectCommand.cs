using MediatR;
using ATMS.Application.Security;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[SuperAdminAccess]
public class DeleteWorkProjectCommand : IRequest, IProjectScopedRequest
{
    public required Guid Id { get; set; }

    Guid IProjectScopedRequest.ProjectId => Id;
}
