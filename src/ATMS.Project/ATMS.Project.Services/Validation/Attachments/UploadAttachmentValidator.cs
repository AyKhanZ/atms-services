using ATMS.Infrastructure.Files;
using ATMS.Infrastructure.Options;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ATMS.Project.Services.Validation.Attachments;

public class UploadAttachmentValidator : AbstractValidator<UploadAttachmentCommand>
{
    private readonly IWorkProjectRepository _workProjectRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IFileSignatureService _fileSignatureService;
    private readonly AttachmentsOptions _options;

    public UploadAttachmentValidator(
        IConfiguration configuration,
        IWorkProjectRepository workProjectRepository,
        IAttachmentRepository attachmentRepository,
        IFileSignatureService fileSignatureService)
    {
        _workProjectRepository = workProjectRepository;
        _attachmentRepository = attachmentRepository;
        _fileSignatureService = fileSignatureService;
        _options = configuration.GetSection(nameof(AttachmentsOptions)).Get<AttachmentsOptions>()
                   ?? new AttachmentsOptions { RootPath = string.Empty };

        RuleFor(command => command.ProjectId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired)
            .MustAsync(IsProjectExistsAsync).WithMessage(WorkProjectMessages.NotFound);

        RuleFor(command => command.WorkTaskId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.TaskRequired)
            .MustAsync(IsWorkTaskExistsAsync).WithMessage(WorkTaskMessages.NotFound)
            .When(command => command.ProjectId != Guid.Empty, ApplyConditionTo.CurrentValidator)
            .MustAsync(HasRoomForFileAsync)
            .WithMessage(_ => string.Format(AttachmentMessages.TooManyFiles, _options.MaxFilesPerOwner));

        RuleFor(command => command.File).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage(AttachmentMessages.FileRequired)
            .Must(file => file!.Length > 0).WithMessage(AttachmentMessages.FileEmpty)
            .Must(file => file!.Length <= _options.MaxFileSizeBytes)
            .WithMessage(_ => string.Format(AttachmentMessages.FileTooLarge, _options.MaxFileSizeBytes / (1024 * 1024)))
            .Must(file => _fileSignatureService.IsAllowedExtension(ExtensionOf(file!)))
            .WithMessage(AttachmentMessages.FileTypeUnsupported)
            .MustAsync((file, cancellationToken) =>
                _fileSignatureService.MatchesExtensionAsync(file!, ExtensionOf(file!), cancellationToken))
            .WithMessage(AttachmentMessages.FileContentMismatch);
    }

    private static string ExtensionOf(IFormFile file)
    {
        return Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
    }

    private Task<bool> IsProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return _workProjectRepository.IsExistAsync(project => project.Id == projectId, cancellationToken);
    }

    private Task<bool> IsWorkTaskExistsAsync(UploadAttachmentCommand command, Guid workTaskId, CancellationToken cancellationToken)
    {
        return _attachmentRepository.IsOwnerTaskLiveAsync(command.ProjectId, workTaskId, cancellationToken);
    }

    private async Task<bool> HasRoomForFileAsync(Guid workTaskId, CancellationToken cancellationToken)
    {
        return await _attachmentRepository.CountByWorkTaskAsync(workTaskId, cancellationToken) < _options.MaxFilesPerOwner;
    }
}
