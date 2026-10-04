using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.Notifications;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Notifications;

public sealed class GetNotificationsRequest : GetKeysetPaginationRequest,
    IRequest<KeysetPagedResult<NotificationModel>>
{
    /// <summary>Only notifications that are not read yet.</summary>
    public bool UnreadOnly { get; init; }
}
