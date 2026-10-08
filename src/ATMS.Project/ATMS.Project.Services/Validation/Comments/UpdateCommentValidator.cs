using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Comments;

public sealed class UpdateCommentValidator : AbstractValidator<UpdateCommentCommand>
{
    private readonly IWorkProjectRepository _projectRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateCommentValidator(
        IWorkProjectRepository projectRepository,
        ICommentRepository commentRepository,
        ICurrentUser currentUser)
    {
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;
        _currentUser = currentUser;

        RuleFor(command => command.ProjectId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired)
            .MustAsync(IsProjectExistsAsync).WithMessage(WorkProjectMessages.NotFound);

        RuleFor(command => command.CommentId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(CommentMessages.CommentRequired)
            .CustomAsync(CheckAuthorAsync)
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

    // one read: no author = no comment, another author = not yours
    private async Task CheckAuthorAsync(
        Guid id,
        ValidationContext<UpdateCommentCommand> context,
        CancellationToken token)
    {
        var authorId = await _commentRepository.GetAuthorIdAsync(context.InstanceToValidate.ProjectId, id, token);
        if (authorId is null)
        {
            context.AddFailure(CommentMessages.NotFound);
        }
        else if (authorId != _currentUser.Id)
        {
            context.AddFailure(CommentMessages.EditOwnOnly);
        }
    }
}
