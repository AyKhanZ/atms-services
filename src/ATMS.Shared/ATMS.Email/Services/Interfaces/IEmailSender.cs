using ATMS.Email.Models;

namespace ATMS.Email.Services.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string to, InviteModel inviteModel, CancellationToken cancellationToken);
    Task SendAsync(string to, ForgotPasswordModel forgotPasswordModel, CancellationToken cancellationToken);
}