using MediatR;

namespace ATMS.Project.Contracts.Commands.Notifications;

public sealed class MarkNotificationUnreadCommand : NotificationCommand, IRequest;
