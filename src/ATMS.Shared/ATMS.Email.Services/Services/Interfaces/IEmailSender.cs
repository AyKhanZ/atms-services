using ATMS.Email.Services.Models;

namespace ATMS.Email.Services.Services.Interfaces;

public interface IEmailSender
{
    Task SendInviteAsync(string to, string subject, InviteModel inviteModel);
}