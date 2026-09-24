using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Attachments;

public class DeleteAttachmentValidator : AbstractValidator<DeleteAttachmentCommand>
{
    private readonly IWorkProjectRepository _workProjectRepository;
    private readonly IAttachmentRepository _attachmentRepository;

    public DeleteAttachmentValidator(
        IWorkProjectRepository workProjectRepository,
        IAttachmentRepository attachmentRepository)
    {
        _workProjectRepository = workProjectRepository;
        _attachmentRepository = attachmentRepository;

        RuleFor(command => command.ProjectId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired)
            .MustAsync(IsProjectExistsAsync).WithMessage(WorkProjectMessages.NotFound);

        RuleFor(command => command.AttachmentId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AttachmentMessages.AttachmentRequired)
            .MustAsync(IsAttachmentExistsAsync).WithMessage(AttachmentMessages.NotFound)
            .When(command => command.ProjectId != Guid.Empty, ApplyConditionTo.CurrentValidator);
    }

    private Task<bool> IsProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return _workProjectRepository.IsExistAsync(project => project.Id == projectId, cancellationToken);
    }

    private Task<bool> IsAttachmentExistsAsync(DeleteAttachmentCommand command, Guid attachmentId, CancellationToken cancellationToken)
    {
        return _attachmentRepository.IsAttachmentExistAsync(command.ProjectId, attachmentId, cancellationToken);
    }
}
