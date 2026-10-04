using ATMS.Project.Contracts.Models.Notifications;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Notifications;

public sealed class GetNotificationSummaryRequest : IRequest<NotificationSummaryModel>;
