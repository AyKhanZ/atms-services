using ATMS.Email.Models;
using FluentEmail.Razor;

var repositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory);
var templatesDirectory = Path.Combine(repositoryRoot, "src", "ATMS.Shared", "ATMS.Email", "Templates");
var outputDirectory = Path.Combine(repositoryRoot, "artifacts", "email-preview");

Directory.CreateDirectory(outputDirectory);

var renderer = new RazorRenderer();
var generated = new List<string>();

await RenderTemplateAsync(
    "InviteTemplate.cshtml",
    "confirm-email.html",
    new InviteModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        Email = "aykhan.zeynalov@baim.az",
        Password = "Baim@2026!",
        Link = "http://localhost:5000/admin/api/v1/account/confirm?token=preview-confirmation-token",
        DeadlineOfToken = DateTime.Now.AddHours(24)
    });

await RenderTemplateAsync(
    "ForgotPasswordTemplate.cshtml",
    "forgot-password.html",
    new ForgotPasswordModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        Email = "aykhan.zeynalov@baim.az",
        Link = "http://localhost:4200/reset-password?token=preview-reset-token",
        DeadlineOfToken = DateTime.Now.AddHours(1)
    });

// Notification emails (Specs/14-notifications.md, "Почта"). A long title shows how the card wraps.
const string taskLink = "http://localhost:4200/projects/preview-project/tickets/preview-ticket/tasks/preview-task";

await RenderTemplateAsync(
    "TaskAssignedTemplate.cshtml",
    "notification-task-assigned.html",
    new TaskAssignedModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        ActorName = "Leyla Mammadova",
        TaskLabel = "TASK #41 Payment form validation for the client portal",
        ProjectTitle = "Project Alpha",
        Link = taskLink
    });

await RenderTemplateAsync(
    "MentionedTemplate.cshtml",
    "notification-mentioned.html",
    new MentionedModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        ActorName = "Leyla Mammadova",
        TaskLabel = "SUBTASK #42 Check the payment form on staging",
        ProjectTitle = "Project Alpha",
        Link = $"{taskLink}#comment-preview-comment"
    });

await RenderTemplateAsync(
    "DueTodayTemplate.cshtml",
    "notification-due-today.html",
    new DueTodayModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        TaskLabel = "TASK #41 Payment form validation for the client portal",
        ProjectTitle = "Project Alpha",
        Link = taskLink
    });

await RenderTemplateAsync(
    "TaskOverdueTemplate.cshtml",
    "notification-task-overdue.html",
    new TaskOverdueModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        TaskLabel = "TASK #41 Payment form validation for the client portal",
        ProjectTitle = "Project Alpha",
        Deadline = "5 Oct 2026",
        Link = taskLink
    });

await RenderTemplateAsync(
    "AddedToProjectTemplate.cshtml",
    "notification-added-to-project.html",
    new AddedToProjectModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        ActorName = "Leyla Mammadova",
        ProjectTitle = "Project Alpha",
        Link = "http://localhost:4200/projects/preview-project"
    });

Console.WriteLine("Email previews generated:");
foreach (var path in generated)
{
    Console.WriteLine(path);
}

async Task RenderTemplateAsync<TModel>(string templateFileName, string outputFileName, TModel model)
{
    var templatePath = Path.Combine(templatesDirectory, templateFileName);
    var outputPath = Path.Combine(outputDirectory, outputFileName);
    var template = await File.ReadAllTextAsync(templatePath);
    var html = await renderer.ParseAsync(template, model, true);
    await File.WriteAllTextAsync(outputPath, html);
    generated.Add(outputPath);
}

static string FindRepositoryRoot(string startDirectory)
{
    var current = new DirectoryInfo(startDirectory);

    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "atms-services.sln")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    throw new DirectoryNotFoundException("Could not find atms-services.sln.");
}
