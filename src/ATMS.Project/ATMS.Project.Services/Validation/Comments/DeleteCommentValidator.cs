using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Comments;

public sealed class DeleteCommentValidator : AbstractValidator<DeleteCommentCommand>
{
    private readonly IWorkProjectRepository _projectRepository;
    private readonly ICommentRepository _commentRepository;

    public DeleteCommentValidator(IWorkProjectRepository projectRepository, ICommentRepository commentRepository)
    {
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;

        RuleFor(command => command.ProjectId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired)
            .MustAsync(IsProjectExistsAsync).WithMessage(WorkProjectMessages.NotFound);

        RuleFor(command => command.CommentId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(CommentMessages.CommentRequired)
            .MustAsync(IsCommentExistsAsync).WithMessage(CommentMessages.NotFound)
            .When(command => command.ProjectId != Guid.Empty, ApplyConditionTo.CurrentValidator);
    }

    private Task<bool> IsProjectExistsAsync(Guid id, CancellationToken token)
    {
        return _projectRepository.IsExistAsync(project => project.Id == id, token);
    }

    private Task<bool> IsCommentExistsAsync(DeleteCommentCommand command, Guid id, CancellationToken token)
    {
        return _commentRepository.IsLiveCommentAsync(command.ProjectId, id, token);
    }
}
