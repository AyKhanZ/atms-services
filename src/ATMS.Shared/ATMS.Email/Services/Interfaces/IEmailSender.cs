using ATMS.Email.Models;

namespace ATMS.Email.Services.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string to, InviteModel inviteModel, CancellationToken cancellationToken);
    Task SendAsync(string to, ForgotPasswordModel forgotPasswordModel, CancellationToken cancellationToken);
    Task SendAsync(string to, TaskAssignedModel model, CancellationToken cancellationToken);
    Task SendAsync(string to, MentionedModel model, CancellationToken cancellationToken);
    Task SendAsync(string to, DueTodayModel model, CancellationToken cancellationToken);
    Task SendAsync(string to, TaskOverdueModel model, CancellationToken cancellationToken);
    Task SendAsync(string to, AddedToProjectModel model, CancellationToken cancellationToken);
}
