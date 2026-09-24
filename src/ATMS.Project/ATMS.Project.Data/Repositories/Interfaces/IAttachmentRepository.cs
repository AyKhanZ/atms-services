using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Attachments;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IAttachmentRepository
{
    Task<AttachmentListItem[]> GetManyAsync(
        ACriteria<WorkTask> ownerTasks,
        int limit,
        CancellationToken cancellationToken);

    Task<AttachmentListItem?> GetAsync(Guid attachmentId, CancellationToken cancellationToken);

    Task<AttachmentTicketCount[]> GetTicketCountsAsync(
        ACriteria<WorkTask> ownerTasks,
        CancellationToken cancellationToken);

    Task<Attachment?> FindAsync(Guid projectId, Guid attachmentId, CancellationToken cancellationToken);

    Task<bool> IsAttachmentExistAsync(Guid projectId, Guid attachmentId, CancellationToken cancellationToken);

    Task<int> CountByWorkTaskAsync(Guid workTaskId, CancellationToken cancellationToken);

    Task AddAsync(Attachment attachment, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
