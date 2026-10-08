using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;

namespace ATMS.Email.Services;

public sealed class EmailSender(IFluentEmailFactory fluentEmailFactory, ILogger<EmailSender> logger) : IEmailSender
{
    private const string InviteSubject = "Confirm your account";
    private const string ForgotPasswordSubject = "Reset your password";

    private const string InviteTemplate = "InviteTemplate.cshtml";
    private const string ForgotPasswordTemplate = "ForgotPasswordTemplate.cshtml";
    private const string TaskAssignedTemplate = "TaskAssignedTemplate.cshtml";
    private const string MentionedTemplate = "MentionedTemplate.cshtml";
    private const string DueTodayTemplate = "DueTodayTemplate.cshtml";
    private const string TaskOverdueTemplate = "TaskOverdueTemplate.cshtml";
    private const string AddedToProjectTemplate = "AddedToProjectTemplate.cshtml";

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

    public Task SendAsync(string to, TaskAssignedModel model, CancellationToken cancellationToken)
    {
        return SendTemplateAsync(
            to,
            $"{model.ActorName} assigned you {model.TaskLabel}",
            TaskAssignedTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, MentionedModel model, CancellationToken cancellationToken)
    {
        return SendTemplateAsync(
            to,
            $"{model.ActorName} mentioned you in {model.TaskLabel}",
            MentionedTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, DueTodayModel model, CancellationToken cancellationToken)
    {
        return SendTemplateAsync(
            to,
            $"{model.TaskLabel} is due today",
            DueTodayTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, TaskOverdueModel model, CancellationToken cancellationToken)
    {
        return SendTemplateAsync(
            to,
            $"{model.TaskLabel} is overdue",
            TaskOverdueTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, AddedToProjectModel model, CancellationToken cancellationToken)
    {
        return SendTemplateAsync(
            to,
            $"{model.ActorName} added you to {model.ProjectTitle}",
            AddedToProjectTemplate,
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
