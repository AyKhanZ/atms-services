using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Criteria.WorkProjectParticipants;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public sealed class WorkTaskBoardRepository(ProjectDbContext context, IWorkTaskRepository workTaskRepository) : IWorkTaskBoardRepository
{
    public async Task<WorkTasksQueryResult> GetManyAsync(
        ICriteria<WorkTask> criteria,
        IKeysetPagination<WorkTask> pagination,
        IReadOnlyCollection<string> languages,
        CancellationToken cancellationToken)
    {
        // only what the card shows, in one query; status and priority names only in the caller's language + english
        var query = criteria.Apply(context.WorkTasks
            .AsNoTracking()
            .Include(task => task.Status)
                .ThenInclude(status => status.Translations
                    .Where(translation => languages.Contains(translation.Language)))
            .Include(task => task.Priority)
                .ThenInclude(priority => priority.Translations
                    .Where(translation => languages.Contains(translation.Language)))
            .Include(task => task.Assignee)
                .ThenInclude(participant => participant.User)
            .Include(task => task.WorkTicket)
            .Include(task => task.ParentWorkTask)
            .Include(task => task.WorkProject)
            .AsSingleQuery());

        var items = await pagination.Apply(query).ToArrayAsync(cancellationToken);
        var page = pagination.ToResult(items);
        var progress = await workTaskRepository.GetProgressByParentAsync(
            page.Items.Select(task => task.Id).ToArray(),
            cancellationToken);

        return new WorkTasksQueryResult(page, progress);
    }

    public Task<Dictionary<int, int>> GetCountsByStatusAsync(
        ICriteria<WorkTask> criteria,
        CancellationToken cancellationToken)
    {
        return criteria.Apply(context.WorkTasks.AsNoTracking())
            .GroupBy(task => task.StatusId)
            .Select(group => new { StatusId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.StatusId, row => row.Count, cancellationToken);
    }

    public async Task<WorkTaskBoardAssignee[]> GetAssigneesAsync(
        ICriteria<WorkProjectParticipant> criteria,
        CancellationToken cancellationToken)
    {
        // only active employees can be chosen; people already on a task stay there
        var activeEmployees = new EmployeeUsersCriteria().And(new ActiveUsersCriteria()).Apply(context.Users);
        var employees = new ParticipantsAmongUsersCriteria(activeEmployees);
        var participants = employees.Apply(criteria.Apply(context.WorkProjectParticipants.AsNoTracking()));

        var people = await participants
            .Select(participant => new
            {
                participant.User.Id,
                participant.User.Name,
                participant.User.Surname,
                participant.User.AvatarPath
            })
            .Distinct()
            .OrderBy(person => person.Name)
            .ThenBy(person => person.Surname)
            .ToArrayAsync(cancellationToken);

        return people
            .Select(person => new WorkTaskBoardAssignee(person.Id, person.Name, person.Surname, person.AvatarPath))
            .ToArray();
    }
}
