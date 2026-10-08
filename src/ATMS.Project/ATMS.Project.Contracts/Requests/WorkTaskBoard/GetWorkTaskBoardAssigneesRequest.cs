using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Users;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkTaskBoard;

/// <summary>People the "Assigned to" filter offers: staff of the chosen projects, or of all of the caller's.</summary>
[Access(PermissionEnum.ProjectView)]
public sealed class GetWorkTaskBoardAssigneesRequest : IRequest<PersonModel[]>
{
    public Guid[] ProjectIds { get; init; } = [];
}
