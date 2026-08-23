using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkGroups;

public class CreateWorkGroupHandler(
    ICurrentUser currentUser,
    IWorkProjectRepository workProjectRepository,
    IWorkGroupRepository workGroupRepository)
    : IRequestHandler<CreateWorkGroupCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateWorkGroupCommand command,
        CancellationToken cancellationToken)
    {
        if (!await workProjectRepository.IsExistAsync(
                x => x.Id == command.ProjectId,
                cancellationToken))
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        if (command.ParentWorkGroupId.HasValue &&
            !await workGroupRepository.IsRootExistAsync(
                command.ProjectId,
                command.ParentWorkGroupId.Value,
                cancellationToken))
        {
            throw new EntityException(EntityErrorType.NotFound, WorkGroupMessages.ParentGroupNotFound);
        }

        var workGroup = new WorkGroup
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            ParentWorkGroupId = command.ParentWorkGroupId,
            WorkProjectId = command.ProjectId,
            StatusId = (int)WorkGroupStatusEnum.Planned,
            CreatedById = currentUser.Id
        };

        await workGroupRepository.CreateAsync(workGroup, cancellationToken);

        return workGroup.Id;
    }
}
