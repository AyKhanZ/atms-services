using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Notifications;

public class MarkNotificationReadValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadValidator(INotificationRepository notificationRepository, ICurrentUser currentUser)
    {
        RuleFor(command => command)
            .SetValidator(new NotificationCommandValidator(notificationRepository, currentUser));
    }
}
