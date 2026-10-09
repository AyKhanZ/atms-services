using System.Globalization;
using ATMS.Application.Localization;
using ATMS.Email.Models;
using ATMS.Email.Resources;
using ATMS.Email.Services.Interfaces;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;

namespace ATMS.Email.Services;

public sealed class EmailSender(IFluentEmailFactory fluentEmailFactory, ILogger<EmailSender> logger) : IEmailSender
{
    private const string InviteTemplate = "InviteTemplate.cshtml";
    private const string ForgotPasswordTemplate = "ForgotPasswordTemplate.cshtml";
    private const string TaskAssignedTemplate = "TaskAssignedTemplate.cshtml";
    private const string MentionedTemplate = "MentionedTemplate.cshtml";
    private const string DueTodayTemplate = "DueTodayTemplate.cshtml";
    private const string TaskOverdueTemplate = "TaskOverdueTemplate.cshtml";
    private const string AddedToProjectTemplate = "AddedToProjectTemplate.cshtml";

    public Task SendAsync(string to, string language, InviteModel model, CancellationToken cancellationToken)
    {
        return SendLocalizedAsync(to, language, () => EmailMessages.InviteSubject, InviteTemplate, model, cancellationToken);
    }

    public Task SendAsync(string to, string language, ForgotPasswordModel model, CancellationToken cancellationToken)
    {
        return SendLocalizedAsync(
            to,
            language,
            () => EmailMessages.ForgotPasswordSubject,
            ForgotPasswordTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, string language, TaskAssignedModel model, CancellationToken cancellationToken)
    {
        return SendLocalizedAsync(
            to,
            language,
            () => string.Format(CultureInfo.CurrentCulture, EmailMessages.TaskAssignedSubject, model.ActorName, model.TaskLabel),
            TaskAssignedTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, string language, MentionedModel model, CancellationToken cancellationToken)
    {
        return SendLocalizedAsync(
            to,
            language,
            () => string.Format(CultureInfo.CurrentCulture, EmailMessages.MentionedSubject, model.ActorName, model.TaskLabel),
            MentionedTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, string language, DueTodayModel model, CancellationToken cancellationToken)
    {
        return SendLocalizedAsync(
            to,
            language,
            () => string.Format(CultureInfo.CurrentCulture, EmailMessages.DueTodaySubject, model.TaskLabel),
            DueTodayTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, string language, TaskOverdueModel model, CancellationToken cancellationToken)
    {
        return SendLocalizedAsync(
            to,
            language,
            () => string.Format(CultureInfo.CurrentCulture, EmailMessages.TaskOverdueSubject, model.TaskLabel),
            TaskOverdueTemplate,
            model,
            cancellationToken);
    }

    public Task SendAsync(string to, string language, AddedToProjectModel model, CancellationToken cancellationToken)
    {
        return SendLocalizedAsync(
            to,
            language,
            () => string.Format(CultureInfo.CurrentCulture, EmailMessages.AddedToProjectSubject, model.ActorName, model.ProjectTitle),
            AddedToProjectTemplate,
            model,
            cancellationToken);
    }

    // resx and the template follow CurrentUICulture; put it back so the next recipient is not stuck with this one
    private async Task SendLocalizedAsync<TModel>(
        string to,
        string language,
        Func<string> subject,
        string templateName,
        TModel model,
        CancellationToken cancellationToken)
    {
        var culture = new CultureInfo(SupportedLanguages.ToCulture(SupportedLanguages.Normalize(language)));
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            await SendTemplateAsync(to, subject(), templateName, model, cancellationToken);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
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
