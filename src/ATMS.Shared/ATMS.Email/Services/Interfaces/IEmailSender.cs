using ATMS.Email.Models;

namespace ATMS.Email.Services.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string to, string language, InviteModel inviteModel, CancellationToken cancellationToken);
    Task SendAsync(string to, string language, ForgotPasswordModel forgotPasswordModel, CancellationToken cancellationToken);
    Task SendAsync(string to, string language, TaskAssignedModel model, CancellationToken cancellationToken);
    Task SendAsync(string to, string language, MentionedModel model, CancellationToken cancellationToken);
    Task SendAsync(string to, string language, DueTodayModel model, CancellationToken cancellationToken);
    Task SendAsync(string to, string language, TaskOverdueModel model, CancellationToken cancellationToken);
    Task SendAsync(string to, string language, AddedToProjectModel model, CancellationToken cancellationToken);
}
