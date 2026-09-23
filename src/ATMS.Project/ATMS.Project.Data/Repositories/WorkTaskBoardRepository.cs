using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Criteria.WorkProjectParticipants;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class WorkTaskBoardRepository(ProjectDbContext context, IWorkTaskRepository workTaskRepository) : IWorkTaskBoardRepository
{
    public async Task<WorkTasksQueryResult> GetManyAsync(
        ICriteria<WorkTask> criteria,
        IKeysetPagination<WorkTask> pagination,
        CancellationToken cancellationToken)
    {
        var query = criteria.Apply(context.WorkTasks
            .AsNoTracking()
            .Include(task => task.Status)
                .ThenInclude(status => status.Translations)
            .Include(task => task.Priority)
                .ThenInclude(priority => priority.Translations)
            .Include(task => task.Assignee)
                .ThenInclude(participant => participant.User)
            .Include(task => task.WorkTicket)
                .ThenInclude(ticket => ticket.WorkGroup)
                .ThenInclude(milestone => milestone.ParentWorkGroup)
            .Include(task => task.ParentWorkTask)
            .Include(task => task.WorkProject)
            .AsSplitQuery());

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
        // Only employees can be assigned work: the rule is the data's, not the caller's.
        var employees = new ParticipantsAmongUsersCriteria(new EmployeeUsersCriteria().Apply(context.Users));
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
