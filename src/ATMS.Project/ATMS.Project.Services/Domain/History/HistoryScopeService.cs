using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.History.Interfaces;
using ATMS.Project.Services.Models.History;
using ATMS.Project.Services.Resources;
using FluentValidation;
using FluentValidation.Results;

namespace ATMS.Project.Services.Domain.History;

// history of a deleted item isn't shown, even though its rows still exist
public sealed class HistoryScopeService(
    IWorkProjectRepository workProjectRepository,
    IWorkTaskRepository workTaskRepository) : IHistoryScopeService
{
    public async Task<HistoryScope> ResolveAsync(
        Guid projectId,
        Guid? workTicketId,
        Guid? workTaskId,
        CancellationToken cancellationToken)
    {
        if (workTicketId.HasValue && workTaskId.HasValue)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(workTaskId), HistoryMessages.ScopeConflict)
            ]);
        }

        if (workTaskId is { } taskId)
        {
            if (!await workTaskRepository.IsWorkTaskExistAsync(projectId, taskId, cancellationToken))
            {
                throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.NotFound);
            }

            return new HistoryScope(HistoryEntityTypeEnum.WorkTask, taskId);
        }

        if (workTicketId is { } ticketId)
        {
            if (!await workTaskRepository.IsWorkTicketExistAsync(projectId, ticketId, cancellationToken))
            {
                throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTicketMessages.NotFound);
            }

            return new HistoryScope(HistoryEntityTypeEnum.WorkTicket, ticketId);
        }

        if (!await workProjectRepository.IsExistAsync(project => project.Id == projectId, cancellationToken))
        {
            throw new EntityException(EntityErrorTypeEnum.NotFound, WorkProjectMessages.NotFound);
        }

        return new HistoryScope(HistoryEntityTypeEnum.Project, projectId);
    }
}
