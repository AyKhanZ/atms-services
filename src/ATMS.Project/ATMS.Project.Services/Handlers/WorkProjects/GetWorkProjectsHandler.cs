using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
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
        var language = CultureHelper.CurrentLanguage;

        return projects.Map(project => Map(project, language));
    }

    private WorkProjectItemModel Map(WorkProject project, string language)
    {
        var model = mapper.Map<WorkProjectItemModel>(project);
        model.ProjectType = project.ProjectType.ToDictionaryModel(project.ProjectType.Translations, language);
        model.ProjectKind = project.ProjectKind.ToDictionaryModel(project.ProjectKind.Translations, language);
        model.ProjectStatus = project.ProjectStatus.ToDictionaryModel(project.ProjectStatus.Translations, language);

        return model;
    }
}
