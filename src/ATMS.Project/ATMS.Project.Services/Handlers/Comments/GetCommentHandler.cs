using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Requests.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Comments.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Comments;

public sealed class GetCommentHandler(
    ICommentRepository comments,
    ICommentModelService models) : IRequestHandler<GetCommentRequest, CommentModel>
{
    public async Task<CommentModel> Handle(GetCommentRequest request, CancellationToken cancellationToken)
    {
        var comment = await comments.GetAsync(request.ProjectId, request.CommentId, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, CommentMessages.NotFound);

        var mapped = await models.BuildAsync(request.ProjectId, [comment], cancellationToken);
        return mapped[comment.Id];
    }
}
