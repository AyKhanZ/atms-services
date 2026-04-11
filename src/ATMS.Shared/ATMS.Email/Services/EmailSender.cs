using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using FluentEmail.Core;

namespace ATMS.Email.Services;

public class EmailSender(IFluentEmail fluentEmail) : IEmailSender
{
    private const string ConfirmEmailSubject = "Complete your account setup";
    private const string ForgotPasswordSubject = "Complete your account setup";

    private static string GetTemplatePath(string templateName) =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", templateName);

    public Task SendAsync(string to, InviteModel inviteModel, CancellationToken cancellationToken)
    {
        return fluentEmail
            .To(to)
            .Subject(ConfirmEmailSubject)
            .UsingTemplateFromFile(GetTemplatePath("InviteTemplate.cshtml"), inviteModel)
            .SendAsync(cancellationToken);
    }

    public Task SendAsync(string to, ForgotPasswordModel forgotPasswordModel, CancellationToken cancellationToken)
    {
        return fluentEmail
            .To(to)
            .Subject(ForgotPasswordSubject)
            .UsingTemplateFromFile(GetTemplatePath("ForgotPasswordTemplate.cshtml"), forgotPasswordModel)
            .SendAsync(cancellationToken);
    }
}
