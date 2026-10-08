using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Infrastructure.Files;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

// not cached: it's just a file path and becomes wrong once the file is deleted
public sealed class GetAttachmentContentHandler(
    IAttachmentRepository attachmentRepository,
    IFileStorage fileStorage,
    IFileSignatureService fileSignatureService) : IRequestHandler<GetAttachmentContentRequest, AttachmentContentModel>
{
    public async Task<AttachmentContentModel> Handle(GetAttachmentContentRequest request, CancellationToken cancellationToken)
    {
        var attachment = await attachmentRepository.GetStoredAsync(request.ProjectId, request.AttachmentId, cancellationToken)
                         ?? throw new EntityException(EntityErrorTypeEnum.NotFound, AttachmentMessages.NotFound);

        var physicalPath = fileStorage.GetFullPath(attachment.RelativePath);
        if (!File.Exists(physicalPath))
        {
            throw new EntityException(EntityErrorTypeEnum.NotFound, AttachmentMessages.FileMissing);
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
