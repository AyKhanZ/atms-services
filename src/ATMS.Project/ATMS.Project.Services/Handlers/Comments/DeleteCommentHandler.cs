using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Comments;

public sealed class DeleteCommentHandler(
    ICommentRepository comments,
    ICurrentUser currentUser) : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await comments.FindAsync(command.ProjectId, command.CommentId, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, CommentMessages.NotFound);

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.DeletedById = currentUser.Id;
        await comments.SaveChangesAsync(cancellationToken);
    }
}
