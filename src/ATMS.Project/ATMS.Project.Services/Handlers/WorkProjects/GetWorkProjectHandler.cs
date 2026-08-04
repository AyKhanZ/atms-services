using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Contracts.Requests.WorkProjects;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class GetWorkProjectHandler(
    ICurrentUser currentUser,
    IWorkProjectRepository workProjectRepository,
    IMapper mapper)
    : IRequestHandler<GetWorkProjectRequest, WorkProjectModel>
{
    public async Task<WorkProjectModel> Handle(
        GetWorkProjectRequest request,
        CancellationToken cancellationToken)
    {
        var criteria = new AccessibleWorkProjectsCriteria(currentUser.Id, currentUser.RoleId);
        var project = await workProjectRepository.GetAsync(request.Id, criteria, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);

        var language = CultureHelper.CurrentLanguage;
        var model = mapper.Map<WorkProjectModel>(project);
        model.ProjectType = project.ProjectType.ToDictionaryModel(project.ProjectType.Translations, language);
        model.ProjectKind = project.ProjectKind.ToDictionaryModel(project.ProjectKind.Translations, language);
        model.ProjectStatus = project.ProjectStatus.ToDictionaryModel(project.ProjectStatus.Translations, language);

        return model;
    }
}
