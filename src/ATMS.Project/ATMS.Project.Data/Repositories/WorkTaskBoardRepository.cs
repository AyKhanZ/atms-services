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
        var query = criteria.Apply(Cards(languages));

        var items = await pagination.Apply(query).ToArrayAsync(cancellationToken);
        var page = pagination.ToResult(items);
        var progress = await workTaskRepository.GetProgressByParentAsync(
            page.Items.Select(task => task.Id).ToArray(),
            cancellationToken);

        return new WorkTasksQueryResult(page, progress);
    }

    public async Task<WorkTaskCalendarResult> GetCalendarAsync(
        ICriteria<WorkTask> criteria,
        int cardsPerDeadline,
        IReadOnlyCollection<string> languages,
        CancellationToken cancellationToken)
    {
        // every task of the range is one small row; full cards are read only for the first few of each deadline.
        // The order is the day panel's (State, then Id), so its first page starts with the same cards
        var rows = await criteria.Apply(context.WorkTasks.AsNoTracking())
            .Where(task => task.Deadline != null)
            .OrderBy(task => task.StatusId)
            .ThenBy(task => task.Id)
            .Select(task => new { task.Id, Deadline = task.Deadline!.Value })
            .ToArrayAsync(cancellationToken);
        var groups = rows
            .GroupBy(row => row.Deadline)
            .OrderBy(group => group.Key)
            .Select(group => new
            {
                Deadline = group.Key,
                Total = group.Count(),
                Ids = group.Take(cardsPerDeadline).Select(row => row.Id).ToArray()
            })
            .ToArray();
        var shownIds = groups.SelectMany(group => group.Ids).ToArray();

        var cards = await Cards(languages)
            .Where(task => shownIds.Contains(task.Id))
            .ToDictionaryAsync(task => task.Id, cancellationToken);
        var progress = await workTaskRepository.GetProgressByParentAsync(shownIds, cancellationToken);

        // a task deleted between the two reads is left out of its group, the count catches up on the next load
        return new WorkTaskCalendarResult(
            groups
                .Select(group => new WorkTaskDeadlineGroup(
                    group.Deadline,
                    group.Total,
                    group.Ids.Where(cards.ContainsKey).Select(id => cards[id]).ToArray()))
                .ToArray(),
            progress);
    }

    public Task<Dictionary<int, int>> GetCountsByColumnAsync(
        ICriteria<WorkTask> criteria,
        CancellationToken cancellationToken)
    {
        return criteria.Apply(context.WorkTasks.AsNoTracking())
            .GroupBy(task => task.BoardColumnId)
            .Select(group => new { BoardColumnId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.BoardColumnId, row => row.Count, cancellationToken);
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

    // only what the card shows, in one query; status and priority names only in the caller's language + english
    private IQueryable<WorkTask> Cards(IReadOnlyCollection<string> languages) =>
        context.WorkTasks
            .AsNoTracking()
            .Include(task => task.Status)
                .ThenInclude(status => status.Translations
                    .Where(translation => languages.Contains(translation.Language)))
            .Include(task => task.BoardColumn)
                .ThenInclude(column => column.Translations
                    .Where(translation => languages.Contains(translation.Language)))
            .Include(task => task.Priority)
                .ThenInclude(priority => priority.Translations
                    .Where(translation => languages.Contains(translation.Language)))
            .Include(task => task.Assignee)
                .ThenInclude(participant => participant.User)
            .Include(task => task.WorkTicket)
            .Include(task => task.ParentWorkTask)
            .Include(task => task.WorkProject)
            .AsSingleQuery();
}
