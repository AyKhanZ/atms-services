using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Comments;

public sealed class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    private readonly IWorkProjectRepository _projectRepository;
    private readonly ICommentRepository _commentRepository;

    public CreateCommentValidator(IWorkProjectRepository projectRepository, ICommentRepository commentRepository)
    {
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;

        RuleFor(command => command.ProjectId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired)
            .MustAsync(IsProjectExistsAsync).WithMessage(WorkProjectMessages.NotFound);

        RuleFor(command => command.WorkTaskId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.TaskRequired)
            .MustAsync(IsTaskExistsAsync).WithMessage(WorkTaskMessages.NotFound)
            .When(command => command.ProjectId != Guid.Empty, ApplyConditionTo.CurrentValidator);

        RuleFor(command => command.Text).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(CommentMessages.TextRequired)
            .Must(text => !string.IsNullOrWhiteSpace(text)).WithMessage(CommentMessages.TextRequired)
            .MaximumLength(2000).WithMessage(CommentMessages.TextTooLong);
    }

    private Task<bool> IsProjectExistsAsync(Guid id, CancellationToken token)
    {
        return _projectRepository.IsExistAsync(project => project.Id == id, token);
    }

    private Task<bool> IsTaskExistsAsync(CreateCommentCommand command, Guid id, CancellationToken token)
    {
        return _commentRepository.IsOwnerTaskLiveAsync(command.ProjectId, id, token);
    }
}
