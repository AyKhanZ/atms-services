using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class AttachmentRepository(ProjectDbContext context) : IAttachmentRepository
{
    public Task<AttachmentListItem[]> GetManyAsync(
        ACriteria<WorkTask> ownerTasks,
        int limit,
        CancellationToken cancellationToken)
    {
        return Project(context.Attachments.AsNoTracking(), ownerTasks.Apply(context.WorkTasks.AsNoTracking()))
            .Take(limit)
            .ToArrayAsync(cancellationToken);
    }

    public Task<AttachmentListItem?> GetAsync(Guid attachmentId, CancellationToken cancellationToken)
    {
        return Project(
                context.Attachments.AsNoTracking().Where(attachment => attachment.Id == attachmentId),
                context.WorkTasks.AsNoTracking())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<AttachmentTicketCount[]> GetTicketCountsAsync(
        ACriteria<WorkTask> ownerTasks,
        CancellationToken cancellationToken)
    {
        var counts = context.Attachments
            .Where(attachment => attachment.OwnerType == AttachmentOwnerTypeEnum.Task)
            .Join(
                ownerTasks.Apply(context.WorkTasks),
                attachment => attachment.OwnerId,
                task => task.Id,
                (_, task) => task.WorkTicketId)
            .GroupBy(workTicketId => workTicketId)
            .Select(group => new { WorkTicketId = group.Key, FileCount = group.Count() });

        return context.WorkTickets
            .AsNoTracking()
            .Where(ticket => ticket.WorkGroup.ParentWorkGroupId != null)
            .Join(
                counts,
                ticket => ticket.Id,
                count => count.WorkTicketId,
                (ticket, count) => new AttachmentTicketCount(
                    ticket.WorkGroup.ParentWorkGroupId!.Value,
                    ticket.WorkGroup.ParentWorkGroup!.Title,
                    ticket.WorkGroup.ParentWorkGroup.CreatedAt,
                    ticket.WorkGroupId,
                    ticket.WorkGroup.Title,
                    ticket.WorkGroup.CreatedAt,
                    ticket.Id,
                    ticket.Code,
                    ticket.Title,
                    ticket.CreatedAt,
                    count.FileCount))
            .ToArrayAsync(cancellationToken);
    }

    public Task<Attachment?> FindAsync(Guid projectId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var ownerTasks = LiveProjectTasks(projectId);

        return context.Attachments.FirstOrDefaultAsync(
            attachment => attachment.Id == attachmentId &&
                          attachment.OwnerType == AttachmentOwnerTypeEnum.Task &&
                          ownerTasks.Any(task => task.Id == attachment.OwnerId),
            cancellationToken);
    }

    public Task<bool> IsAttachmentExistAsync(Guid projectId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var ownerTasks = LiveProjectTasks(projectId);

        return context.Attachments.AnyAsync(
            attachment => attachment.Id == attachmentId &&
                          attachment.OwnerType == AttachmentOwnerTypeEnum.Task &&
                          ownerTasks.Any(task => task.Id == attachment.OwnerId),
            cancellationToken);
    }

    public Task<int> CountByWorkTaskAsync(Guid workTaskId, CancellationToken cancellationToken)
    {
        return context.Attachments.CountAsync(
            attachment => attachment.OwnerType == AttachmentOwnerTypeEnum.Task && attachment.OwnerId == workTaskId,
            cancellationToken);
    }

    public async Task AddAsync(Attachment attachment, CancellationToken cancellationToken)
    {
        await context.Attachments.AddAsync(attachment, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<WorkTask> LiveProjectTasks(Guid projectId)
    {
        return new WorkTasksOfLiveWorkCriteria()
            .Apply(context.WorkTasks)
            .Where(task => task.WorkProjectId == projectId);
    }

    // The author is read past the soft-delete filter: a file stays in the list after the person
    // who uploaded it leaves, and an inner join on the filtered users would silently drop it.
    // IgnoreQueryFilters is not per table — it switches off every filter in the query, so the
    // deleted files and deleted tasks it would let back in are left out here by hand.
    private IQueryable<AttachmentListItem> Project(IQueryable<Attachment> attachments, IQueryable<WorkTask> ownerTasks)
    {
        return
            from attachment in attachments
            where attachment.OwnerType == AttachmentOwnerTypeEnum.Task && !attachment.IsDeleted
            join task in ownerTasks on attachment.OwnerId equals task.Id
            where !task.IsDeleted
            join author in context.Users.IgnoreQueryFilters() on attachment.CreatedById equals author.Id
            orderby attachment.CreatedAt descending, attachment.Id
            select new AttachmentListItem(
                attachment.Id,
                attachment.FileName,
                attachment.ContentType,
                attachment.Size,
                attachment.CreatedAt,
                new AttachmentAuthor(author.Id, author.Name, author.Surname, author.AvatarPath),
                new AttachmentOwnerTask(task.Id, task.Code, task.Title),
                task.ParentWorkTask == null
                    ? null
                    : new AttachmentOwnerTask(task.ParentWorkTask.Id, task.ParentWorkTask.Code, task.ParentWorkTask.Title));
    }
}
