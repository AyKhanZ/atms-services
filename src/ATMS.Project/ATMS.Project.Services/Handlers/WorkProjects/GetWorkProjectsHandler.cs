using ATMS.Application.Interfaces;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Contracts.Requests.WorkProjects;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class GetWorkProjectsHandler(
    ICurrentUser currentUser,
    IWorkProjectRepository workProjectRepository,
    IMapper mapper)
    : IRequestHandler<GetWorkProjectsRequest, PagedResult<WorkProjectItemModel>>
{
    public async Task<PagedResult<WorkProjectItemModel>> Handle(GetWorkProjectsRequest request, CancellationToken cancellationToken)
    {
        var filter = mapper.Map<WorkProjectsFilter>(request);
        var criteria = filter.And(new AccessibleWorkProjectsCriteria(currentUser.Id, currentUser.RoleId));

        var pagination = new PaginationCriteria<WorkProject>(request.Page, request.PageSize);
        var projects = await workProjectRepository.GetAsync(criteria, pagination, cancellationToken);

        return projects.Map(mapper.Map<WorkProjectItemModel>);
    }
}
