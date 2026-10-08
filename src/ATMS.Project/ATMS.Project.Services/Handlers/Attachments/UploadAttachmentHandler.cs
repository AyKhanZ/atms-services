using Microsoft.Extensions.Options;
using System.Globalization;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Enums;
using ATMS.Infrastructure.Files;
using ATMS.Infrastructure.Options;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Attachments.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

public sealed class UploadAttachmentHandler(
    IAttachmentRepository attachmentRepository,
    IFileStorage fileStorage,
    IFileSignatureService fileSignatureService,
    IAttachmentFileNameService fileNameService,
    IOptions<AttachmentsOptions> attachmentsOptions,
    IMapper mapper) : IRequestHandler<UploadAttachmentCommand, AttachmentModel>
{
    private readonly AttachmentsOptions _options = attachmentsOptions.Value;

    public async Task<AttachmentModel> Handle(UploadAttachmentCommand command, CancellationToken cancellationToken)
    {
        var file = command.File!;
        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var now = DateTime.UtcNow;

        // project/month folders: one project is easy to back up and no folder gets thousands of files
        var directory = string.Join(
            '/',
            command.ProjectId.ToString("N"),
            now.ToString("yyyy", CultureInfo.InvariantCulture),
            now.ToString("MM", CultureInfo.InvariantCulture));
        var relativePath = await fileStorage.SaveAsync(file, directory, extension, cancellationToken);

        var attachment = new Attachment
        {
            OwnerType = (int)AttachmentOwnerTypeEnum.Task,
            OwnerId = command.WorkTaskId,
            FileName = fileNameService.FromUpload(file.FileName, extension),
            RelativePath = relativePath,
            ContentType = fileSignatureService.GetContentType(extension),
            Size = file.Length
        };

        bool added;
        try
        {
            added = await attachmentRepository.AddWithinLimitAsync(attachment, _options.MaxFilesPerOwner, cancellationToken);
        }
        catch
        {
            await fileStorage.DeleteAsync(relativePath, CancellationToken.None);
            throw;
        }

        // another upload took the last place while this file was being written
        if (!added)
        {
            await fileStorage.DeleteAsync(relativePath, CancellationToken.None);
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(UploadAttachmentCommand.WorkTaskId),
                    string.Format(AttachmentMessages.TooManyFiles, _options.MaxFilesPerOwner))
            ]);
        }

        var item = await attachmentRepository.GetAsync(attachment.Id, cancellationToken)
                   ?? throw new EntityException(EntityErrorTypeEnum.NotFound, AttachmentMessages.NotFound);

        return mapper.Map<AttachmentModel>(item);
    }
}
