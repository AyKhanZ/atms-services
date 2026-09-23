using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Services.Interfaces;
using ATMS.Project.Services.Board.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public class CreateWorkTaskHandler(
    IMapper mapper,
    IWorkTaskRepository workTaskRepository,
    IEntityCodeGenerator codeGenerator,
    IWorkTaskBoardPositionService boardPositionService) : IRequestHandler<CreateWorkTaskCommand, Guid>
{
    public async Task<Guid> Handle(CreateWorkTaskCommand command, CancellationToken cancellationToken)
    {
        WorkTask? parent = null;
        if (command.ParentWorkTaskId.HasValue)
        {
            parent = await workTaskRepository.FindParentAsync(command.ProjectId, command.ParentWorkTaskId.Value, cancellationToken)
                ?? throw new EntityException(EntityErrorType.NotFound, WorkTaskMessages.ParentNotFound);
        }

        var workTask = mapper.Map<WorkTask>(command);
        workTask.Id = Guid.NewGuid();
        workTask.Code = await codeGenerator.GetNextAsync(cancellationToken);
        workTask.StatusId = (int)WorkTaskStatusEnum.New;
        workTask.WorkTicketId = parent is null ? command.WorkTicketId : parent.WorkTicketId;
        workTask.Rank = boardPositionService.Between(null, await workTaskRepository.GetTopRankAsync(cancellationToken));

        await workTaskRepository.CreateAsync(workTask, cancellationToken);

        return workTask.Id;
    }
}
