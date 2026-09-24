using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

// The file stays on disk: the row is only marked deleted, like every other work item.
public class DeleteAttachmentHandler(
    ICurrentUser currentUser,
    IAttachmentRepository attachmentRepository) : IRequestHandler<DeleteAttachmentCommand>
{
    public async Task Handle(DeleteAttachmentCommand command, CancellationToken cancellationToken)
    {
        var attachment = await attachmentRepository.FindAsync(command.ProjectId, command.AttachmentId, cancellationToken)
                         ?? throw new EntityException(EntityErrorType.NotFound, AttachmentMessages.NotFound);

        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTime.UtcNow;
        attachment.DeletedById = currentUser.Id;

        await attachmentRepository.SaveChangesAsync(cancellationToken);
    }
}
