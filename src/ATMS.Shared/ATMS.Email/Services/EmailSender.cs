using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;

namespace ATMS.Email.Services;

public class EmailSender(IFluentEmailFactory fluentEmailFactory, ILogger<EmailSender> logger) : IEmailSender
{
    private const string InviteSubject = "Confirm your account";
    private const string ForgotPasswordSubject = "Reset your password";

    private const string InviteTemplate = "InviteTemplate.cshtml";
    private const string ForgotPasswordTemplate = "ForgotPasswordTemplate.cshtml";

    public Task SendAsync(string to, InviteModel model, CancellationToken cancellationToken)
    {
        return SendTemplateAsync(
            to,
            InviteSubject,
            InviteTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, ForgotPasswordModel model, CancellationToken cancellationToken)
    {
        return SendTemplateAsync(
            to,
            ForgotPasswordSubject,
            ForgotPasswordTemplate,
            model,
            cancellationToken);
    }

    private async Task SendTemplateAsync<TModel>(
        string to,
        string subject,
        string templateName,
        TModel model,
        CancellationToken cancellationToken)
    {
        string? errors = null;

        try
        {
            var sendResponse = await fluentEmailFactory
                .Create()
                .To(to)
                .Subject(subject)
                .UsingTemplateFromFile(Path.Combine(AppContext.BaseDirectory, "Templates", templateName), model)
                .SendAsync(cancellationToken);

            if (sendResponse.Successful)
            {
                return;
            }

            errors = sendResponse.ErrorMessages is null || sendResponse.ErrorMessages.Count == 0
                ? "No SMTP error details were returned." : string.Join("; ", sendResponse.ErrorMessages);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "An exception occurred while sending an email. Subject: {Subject}. Template: {TemplateName}",
                subject,
                templateName);
            throw;
        }

        logger.LogError(
            "Email delivery failed. Subject: {Subject}. Template: {TemplateName}. Errors: {Errors}",
            subject,
            templateName,
            errors);

        throw new InvalidOperationException($"SMTP rejected the email. {errors}");
    }

}
