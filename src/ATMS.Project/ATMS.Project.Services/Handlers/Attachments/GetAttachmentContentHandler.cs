using ATMS.Application.Exceptions.Entity;
using ATMS.Infrastructure.Files;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

// Not cached: the answer is a path to a file on disk, cheap to read and wrong the moment the file
// is deleted.
public class GetAttachmentContentHandler(
    IAttachmentRepository attachmentRepository,
    IFileStorage fileStorage,
    IFileSignatureService fileSignatureService) : IRequestHandler<GetAttachmentContentRequest, AttachmentContentModel>
{
    public async Task<AttachmentContentModel> Handle(GetAttachmentContentRequest request, CancellationToken cancellationToken)
    {
        var attachment = await attachmentRepository.FindAsync(request.ProjectId, request.AttachmentId, cancellationToken)
                         ?? throw new EntityException(EntityErrorType.NotFound, AttachmentMessages.NotFound);

        var physicalPath = fileStorage.GetFullPath(attachment.RelativePath);
        if (!File.Exists(physicalPath))
        {
            throw new EntityException(EntityErrorType.NotFound, AttachmentMessages.FileMissing);
        }

        return new AttachmentContentModel
        {
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            PhysicalPath = physicalPath,
            CanPreview = fileSignatureService.CanPreview(attachment.ContentType)
        };
    }
}
