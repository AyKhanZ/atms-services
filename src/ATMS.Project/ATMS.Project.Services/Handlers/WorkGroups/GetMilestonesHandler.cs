using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkGroups;
using ATMS.Project.Contracts.Requests.WorkGroups;
using ATMS.Project.Data.Criteria.WorkGroups;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkGroups;

public class GetMilestonesHandler(
    IWorkProjectRepository workProjectRepository,
    IWorkGroupRepository workGroupRepository,
    IMapper mapper) : IRequestHandler<GetMilestonesRequest, KeysetPagedResult<MilestoneOptionModel>>
{
    public async Task<KeysetPagedResult<MilestoneOptionModel>> Handle(GetMilestonesRequest request, CancellationToken cancellationToken)
    {
        if (!await workProjectRepository.IsExistAsync(project => project.Id == request.ProjectId, cancellationToken))
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        var criteria = new MilestonesByProjectCriteria(request.ProjectId, request.Search);
        var pagination = new KeysetPaginationCriteria<WorkGroup>(
            request.Cursor,
            request.PageSize,
            request.SortDirection);
        var milestones = await workGroupRepository.GetMilestonesAsync(
            criteria,
            pagination,
            cancellationToken);

        return milestones.Map(mapper.Map<MilestoneOptionModel>);
    }
}
