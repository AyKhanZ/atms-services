using System.Globalization;
using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using ATMS.Infrastructure.Files;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Attachments.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

public class UploadAttachmentHandler(
    IAttachmentRepository attachmentRepository,
    IFileStorage fileStorage,
    IFileSignatureService fileSignatureService,
    IAttachmentFileNameService fileNameService,
    IMapper mapper) : IRequestHandler<UploadAttachmentCommand, AttachmentModel>
{
    public async Task<AttachmentModel> Handle(UploadAttachmentCommand command, CancellationToken cancellationToken)
    {
        var file = command.File!;
        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var now = DateTime.UtcNow;

        // Project first, so one project can be backed up or moved as a folder; month next, so no
        // folder collects tens of thousands of files.
        var directory = string.Join(
            '/',
            command.ProjectId.ToString("N"),
            now.ToString("yyyy", CultureInfo.InvariantCulture),
            now.ToString("MM", CultureInfo.InvariantCulture));
        var relativePath = await fileStorage.SaveAsync(file, directory, extension, cancellationToken);

        var attachment = new Attachment
        {
            OwnerType = AttachmentOwnerTypeEnum.Task,
            OwnerId = command.WorkTaskId,
            FileName = fileNameService.FromUpload(file.FileName, extension),
            RelativePath = relativePath,
            ContentType = fileSignatureService.GetContentType(extension),
            Size = file.Length
        };

        try
        {
            await attachmentRepository.AddAsync(attachment, cancellationToken);
            await attachmentRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await fileStorage.DeleteAsync(relativePath, CancellationToken.None);
            throw;
        }

        var item = await attachmentRepository.GetAsync(attachment.Id, cancellationToken)
                   ?? throw new EntityException(EntityErrorType.NotFound, AttachmentMessages.NotFound);

        return mapper.Map<AttachmentModel>(item);
    }
}
