using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTaskBoard;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkTaskBoard;

/// <summary>People the "Assigned to" filter offers: staff of the chosen projects, or of all of the caller's.</summary>
[Access(PermissionEnum.ProjectView)]
public class GetWorkTaskBoardAssigneesRequest : IRequest<WorkTaskBoardAssigneeModel[]>
{
    public Guid[] ProjectIds { get; init; } = [];
}
