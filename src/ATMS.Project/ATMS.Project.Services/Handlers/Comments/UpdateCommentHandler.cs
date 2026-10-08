using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Comments.Interfaces;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Comments;

public sealed class UpdateCommentHandler(
    ICommentRepository comments,
    ICommentModelService models,
    ICommentNotificationService notifications) : IRequestHandler<UpdateCommentCommand, CommentModel>
{
    public async Task<CommentModel> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await comments.FindAsync(command.ProjectId, command.CommentId, cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, CommentMessages.NotFound);

        var previousText = comment.Text;
        comment.Text = command.Text;
        await notifications.NotifyEditedAsync(command.ProjectId, comment, previousText, cancellationToken);
        await comments.SaveChangesAsync(cancellationToken);

        var mapped = await models.BuildAsync(command.ProjectId, [comment], cancellationToken);
        return mapped[comment.Id];
    }
}
