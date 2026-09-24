using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Attachments.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

public class RenameAttachmentHandler(
    IAttachmentRepository attachmentRepository,
    IAttachmentFileNameService fileNameService) : IRequestHandler<RenameAttachmentCommand>
{
    public async Task Handle(RenameAttachmentCommand command, CancellationToken cancellationToken)
    {
        var attachment = await attachmentRepository.FindAsync(command.ProjectId, command.AttachmentId, cancellationToken)
                         ?? throw new EntityException(EntityErrorType.NotFound, AttachmentMessages.NotFound);

        attachment.FileName = fileNameService.Rename(command.FileName!, attachment.FileName);

        await attachmentRepository.SaveChangesAsync(cancellationToken);
    }
}
