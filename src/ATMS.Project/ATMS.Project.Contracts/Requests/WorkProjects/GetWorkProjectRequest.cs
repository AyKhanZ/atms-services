using ATMS.Project.Contracts.Models.WorkProjects;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkProjects;

public class GetWorkProjectRequest : IRequest<WorkProjectModel>
{
    public Guid Id { get; set; }
}
