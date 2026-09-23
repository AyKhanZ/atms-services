using ATMS.Data.Constants;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Models.WorkTaskBoard;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTaskBoard;

public class GetWorkTaskBoardAssigneesHandler(
    ICurrentUser currentUser,
    IWorkTaskBoardRepository workTaskBoardRepository,
    IMapper mapper) : IRequestHandler<GetWorkTaskBoardAssigneesRequest, WorkTaskBoardAssigneeModel[]>
{
    public async Task<WorkTaskBoardAssigneeModel[]> Handle(GetWorkTaskBoardAssigneesRequest request, CancellationToken cancellationToken)
    {
        var people = await workTaskBoardRepository.GetAssigneesAsync(currentUser.Id, currentUser.RoleId == RoleIds.SuperAdmin, request.ProjectIds, cancellationToken);

        return mapper.Map<WorkTaskBoardAssigneeModel[]>(people);
    }
}
