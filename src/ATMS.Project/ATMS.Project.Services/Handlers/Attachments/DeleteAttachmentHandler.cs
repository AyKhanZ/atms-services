using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

// soft delete, the file stays on disk
public sealed class DeleteAttachmentHandler(
    ICurrentUser currentUser,
    IAttachmentRepository attachmentRepository) : IRequestHandler<DeleteAttachmentCommand>
{
    public async Task Handle(DeleteAttachmentCommand command, CancellationToken cancellationToken)
    {
        var attachment = await attachmentRepository.FindAsync(command.ProjectId, command.AttachmentId, cancellationToken)
                         ?? throw new EntityException(EntityErrorTypeEnum.NotFound, AttachmentMessages.NotFound);

        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTime.UtcNow;
        attachment.DeletedById = currentUser.Id;

        await attachmentRepository.SaveChangesAsync(cancellationToken);
    }
}
