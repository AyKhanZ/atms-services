using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Notifications;

public class NotificationCommandValidator : AbstractValidator<NotificationCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUser _currentUser;

    public NotificationCommandValidator(INotificationRepository notificationRepository, ICurrentUser currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;

        RuleFor(command => command.NotificationId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(NotificationMessages.NotificationRequired)
            .MustAsync(IsNotificationExistsAsync).WithMessage(NotificationMessages.NotFound);
    }

    // Only among the caller's own: someone else's notification reads as one that is not there.
    private Task<bool> IsNotificationExistsAsync(Guid id, CancellationToken token)
    {
        return _notificationRepository.IsExistAsync(_currentUser.Id, id, token);
    }
}
