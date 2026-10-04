using ATMS.Application.Interfaces;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Notifications;
using ATMS.Project.Contracts.Requests.Notifications;
using ATMS.Project.Data.Criteria.Notifications;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Notifications;

public sealed class GetNotificationsHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<GetNotificationsRequest, KeysetPagedResult<NotificationModel>>
{
    public async Task<KeysetPagedResult<NotificationModel>> Handle(
        GetNotificationsRequest request,
        CancellationToken cancellationToken)
    {
        // Always newest first: the list is a feed, and the bell shows its head.
        var pagination = new KeysetPaginationCriteria<NotificationRow>(request.Cursor, request.PageSize, SortDirectionEnum.Desc);
        var page = await notifications.GetManyAsync(
            currentUser.Id,
            mapper.Map<NotificationFilter>(request),
            pagination,
            cancellationToken);

        return new KeysetPagedResult<NotificationModel>
        {
            Items = mapper.Map<NotificationModel[]>(page.Items),
            NextCursor = page.NextCursor,
            HasMore = page.HasMore,
            PageSize = page.PageSize
        };
    }
}
