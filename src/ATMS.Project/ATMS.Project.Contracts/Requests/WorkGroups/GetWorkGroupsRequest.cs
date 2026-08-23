using ATMS.Project.Contracts.Models.WorkGroups;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkGroups;

public class GetWorkGroupsRequest : IRequest<WorkGroupModel[]>
{
    public Guid ProjectId { get; set; }
}
