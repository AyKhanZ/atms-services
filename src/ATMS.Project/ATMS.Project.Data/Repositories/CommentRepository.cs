using ATMS.Data.Criteria;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public sealed class CommentRepository(ProjectDbContext context) : ICommentRepository
{
    public async Task<KeysetPagedResult<Comment>> GetManyAsync(
        Guid projectId,
        ACriteria<Comment> criteria,
        KeysetPaginationCriteria<Comment> pagination,
        CancellationToken cancellationToken)
    {
        var tasks = LiveTasks(projectId);
        var query = criteria.Apply(WithDeleted())
            .Where(comment => tasks.Any(task => task.Id == comment.OwnerId));
        var items = await pagination
            .Apply(query, comment => comment.CreatedAt, comment => comment.Id)
            .ToArrayAsync(cancellationToken);

        return pagination.ToResult(items, comment => comment.CreatedAt, comment => comment.Id);
    }

    public Task<Comment?> GetAsync(Guid projectId, Guid commentId, CancellationToken cancellationToken) =>
        OfProject(WithDeleted(), projectId, commentId).FirstOrDefaultAsync(cancellationToken);

    public Task<Comment?> FindAsync(Guid projectId, Guid commentId, CancellationToken cancellationToken) =>
        OfProject(context.Comments, projectId, commentId).FirstOrDefaultAsync(cancellationToken);

    public Task<Guid?> GetAuthorIdAsync(Guid projectId, Guid commentId, CancellationToken cancellationToken) =>
        OfProject(context.Comments.AsNoTracking(), projectId, commentId)
            .Select(comment => (Guid?)comment.CreatedById)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> IsLiveCommentAsync(Guid projectId, Guid commentId, CancellationToken cancellationToken) =>
        OfProject(context.Comments, projectId, commentId).AnyAsync(cancellationToken);

    public Task<bool> IsOwnerTaskLiveAsync(Guid projectId, Guid workTaskId, CancellationToken cancellationToken) =>
        LiveTasks(projectId).AnyAsync(task => task.Id == workTaskId, cancellationToken);

    public Task<int> CountAsync(Guid workTaskId, CancellationToken cancellationToken) =>
        context.Comments.CountAsync(comment =>
            comment.OwnerType == (int)CommentOwnerTypeEnum.Task &&
            comment.OwnerId == workTaskId, cancellationToken);

    public Task<User[]> GetAuthorsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken) =>
        context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToArrayAsync(cancellationToken);

    public Task<User[]> GetMentionedParticipantsAsync(
        Guid projectId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken) =>
        context.WorkProjectParticipants
            .AsNoTracking()
            .Where(participant => participant.WorkProjectId == projectId && userIds.Contains(participant.UserId))
            .Select(participant => participant.User)
            .Distinct()
            .ToArrayAsync(cancellationToken);

    public async Task<CommentWorkItemReferenceRow[]> GetReferencesAsync(
        IReadOnlyCollection<string> codes,
        ICriteria<WorkProject> accessibleProjects,
        CancellationToken cancellationToken)
    {
        if (codes.Count == 0)
        {
            return [];
        }

        var visibleProjectIds = accessibleProjects
            .Apply(context.WorkProjects.AsNoTracking())
            .Select(project => project.Id);
        // Anonymous rows on every side: a UNION ALL is translated only from plain column projections.
        // One sequence numbers projects, tickets and tasks, so a code names one of them at most.
        var projects = context.WorkProjects
            .AsNoTracking()
            .Where(project => codes.Contains(project.Code) && visibleProjectIds.Contains(project.Id))
            .Select(project => new
            {
                project.Code,
                Kind = CommentReferenceKind.Project,
                IsSubtask = false,
                project.Title,
                StatusId = project.ProjectStatusId,
                ProjectId = project.Id,
                WorkTicketId = (Guid?)null,
                WorkTaskId = (Guid?)null
            });
        var tickets = context.WorkTickets
            .AsNoTracking()
            .Where(ticket => codes.Contains(ticket.Code) && visibleProjectIds.Contains(ticket.WorkProjectId))
            .Select(ticket => new
            {
                ticket.Code,
                Kind = CommentReferenceKind.Ticket,
                IsSubtask = false,
                ticket.Title,
                StatusId = ticket.WorkTicketStatusId,
                ProjectId = ticket.WorkProjectId,
                WorkTicketId = (Guid?)ticket.Id,
                WorkTaskId = (Guid?)null
            });
        var tasks = new WorkTasksOfLiveWorkCriteria()
            .Apply(context.WorkTasks.AsNoTracking())
            .Where(task => codes.Contains(task.Code) && visibleProjectIds.Contains(task.WorkProjectId))
            .Select(task => new
            {
                task.Code,
                Kind = CommentReferenceKind.Task,
                IsSubtask = task.ParentWorkTaskId != null,
                task.Title,
                task.StatusId,
                ProjectId = task.WorkProjectId,
                WorkTicketId = (Guid?)task.WorkTicketId,
                WorkTaskId = (Guid?)task.Id
            });

        var rows = await projects.Concat(tickets).Concat(tasks).ToArrayAsync(cancellationToken);
        return rows
            .Select(row => new CommentWorkItemReferenceRow(
                row.Code,
                row.Kind,
                row.IsSubtask,
                row.Title,
                row.StatusId,
                row.ProjectId,
                row.WorkTicketId,
                row.WorkTaskId))
            .ToArray();
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken) =>
        await context.Comments.AddAsync(comment, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);

    // Deleted comments stay in the discussion as placeholders; only their own filter is lifted, so
    // a deleted task, ticket or project still hides its comments.
    private IQueryable<Comment> WithDeleted() =>
        context.Comments.AsNoTracking().IgnoreQueryFilters([ProjectDbContext.CommentSoftDeleteFilter]);

    private IQueryable<WorkTask> LiveTasks(Guid projectId) =>
        new WorkTasksOfLiveWorkCriteria()
            .Apply(context.WorkTasks)
            .Where(task => task.WorkProjectId == projectId);

    private IQueryable<Comment> OfProject(IQueryable<Comment> comments, Guid projectId, Guid commentId)
    {
        var tasks = LiveTasks(projectId);
        return comments.Where(comment =>
            comment.Id == commentId &&
            comment.OwnerType == (int)CommentOwnerTypeEnum.Task &&
            tasks.Any(task => task.Id == comment.OwnerId));
    }
}
