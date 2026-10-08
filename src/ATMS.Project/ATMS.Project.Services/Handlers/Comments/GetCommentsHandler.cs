using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Requests.Comments;
using ATMS.Project.Data.Criteria.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Comments.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Comments;

public sealed class GetCommentsHandler(
    ICommentRepository comments,
    ICommentModelService models,
    IMapper mapper) : IRequestHandler<GetCommentsRequest, KeysetPagedResult<CommentModel>>
{
    public async Task<KeysetPagedResult<CommentModel>> Handle(
        GetCommentsRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = new KeysetPaginationCriteria<Comment>(request.Cursor, request.PageSize, SortDirectionEnum.Desc);
        var page = await comments.GetManyAsync(
            request.ProjectId,
            mapper.Map<CommentFilter>(request),
            pagination,
            cancellationToken);
        var mapped = await models.BuildAsync(request.ProjectId, page.Items, cancellationToken);

        return new KeysetPagedResult<CommentModel>
        {
            Items = page.Items.Select(item => mapped[item.Id]).ToArray(),
            NextCursor = page.NextCursor,
            HasMore = page.HasMore,
            PageSize = page.PageSize
        };
    }
}
