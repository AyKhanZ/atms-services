using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Models.Notifications;
using ATMS.Project.Contracts.Requests.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Notifications;

public sealed class GetNotificationSummaryHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser) : IRequestHandler<GetNotificationSummaryRequest, NotificationSummaryModel>
{
    public async Task<NotificationSummaryModel> Handle(
        GetNotificationSummaryRequest request,
        CancellationToken cancellationToken) =>
        new() { UnreadCount = await notifications.CountUnreadAsync(currentUser.Id, cancellationToken) };
}
