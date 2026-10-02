using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Comments.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Comments;

public sealed class CreateCommentHandler(
    ICommentRepository comments,
    ICommentModelService models) : IRequestHandler<CreateCommentCommand, CommentModel>
{
    public async Task<CommentModel> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            OwnerType = CommentOwnerTypeEnum.Task,
            OwnerId = command.WorkTaskId,
            Text = command.Text
        };

        await comments.AddAsync(comment, cancellationToken);
        await comments.SaveChangesAsync(cancellationToken);

        var mapped = await models.BuildAsync(command.ProjectId, [comment], cancellationToken);
        return mapped[comment.Id];
    }
}
