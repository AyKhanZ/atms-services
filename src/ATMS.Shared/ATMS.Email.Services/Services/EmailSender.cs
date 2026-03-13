using ATMS.Email.Services.Models;
using ATMS.Email.Services.Services.Interfaces;
using FluentEmail.Core;

namespace ATMS.Email.Services.Services;

public class EmailSender(IFluentEmail fluentEmail) : IEmailSender
{
    public Task SendInviteAsync(string to, string subject, InviteModel inviteModel)
    {
        return fluentEmail
            .To(to)
            .Subject(subject)
            .UsingTemplateFromFile("Templates/InviteTemplate.cshtml", inviteModel)
            .SendAsync();
    }
}