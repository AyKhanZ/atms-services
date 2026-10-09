using System.Globalization;
using ATMS.Application.Localization;
using ATMS.Email.Models;
using ATMS.Email.Resources;
using FluentEmail.Razor;

var repositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory);
var templatesDirectory = Path.Combine(repositoryRoot, "src", "ATMS.Shared", "ATMS.Email", "Templates");
var outputDirectory = Path.Combine(repositoryRoot, "artifacts", "email-preview");

Directory.CreateDirectory(outputDirectory);
foreach (var stale in Directory.EnumerateFiles(outputDirectory, "*.html"))
{
    File.Delete(stale);
}

var renderer = new RazorRenderer();
var generated = new List<string>();
string[] languages = ["en", "ru", "az"];

foreach (var language in languages)
{
    var culture = new CultureInfo(SupportedLanguages.ToCulture(SupportedLanguages.Normalize(language)));
    var previousCulture = CultureInfo.CurrentCulture;
    var previousUiCulture = CultureInfo.CurrentUICulture;
    CultureInfo.CurrentCulture = culture;
    CultureInfo.CurrentUICulture = culture;

    try
    {
    await RenderAsync("InviteTemplate.cshtml", "Invite", () => new InviteModel
    {
        Name = "Nigar",
        Surname = "Huseynova",
        Email = "nigar.huseynova@client.az",
        Password = "Baim@2026!",
        Link = "http://localhost:5000/admin/api/v1/account/confirm?token=preview-confirmation-token",
        DeadlineOfToken = new DateTime(2026, 10, 6, 15, 0, 0),
        InviterName = "Leyla Mammadova",
        ProjectTitle = "Customer portal redesign and payment gateway migration"
    });

    await RenderAsync("ForgotPasswordTemplate.cshtml", "ForgotPassword", () => new ForgotPasswordModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        Email = "aykhan.zeynalov@baim.az",
        Link = "http://localhost:4200/reset-password?token=preview-reset-token",
        DeadlineOfToken = new DateTime(2026, 10, 6, 15, 0, 0)
    });

    const string taskLink = "http://localhost:4200/projects/preview-project/tickets/preview-ticket/tasks/preview-task";
    var taskLabel = $"{EmailMessages.TaskKind} #41 Payment form validation for the client portal";
    var subtaskLabel = $"{EmailMessages.SubtaskKind} #42 Check the payment form on staging";

    await RenderAsync("TaskAssignedTemplate.cshtml", "TaskAssigned", () => new TaskAssignedModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        ActorName = "Leyla Mammadova",
        TaskLabel = taskLabel,
        ProjectTitle = "Project Alpha",
        Link = taskLink
    });

    await RenderAsync("MentionedTemplate.cshtml", "Mentioned", () => new MentionedModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        ActorName = "Leyla Mammadova",
        TaskLabel = subtaskLabel,
        ProjectTitle = "Project Alpha",
        Link = $"{taskLink}#comment-preview-comment"
    });

    await RenderAsync("DueTodayTemplate.cshtml", "DueToday", () => new DueTodayModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        TaskLabel = taskLabel,
        ProjectTitle = "Project Alpha",
        Link = taskLink
    });

    await RenderAsync("TaskOverdueTemplate.cshtml", "TaskOverdue", () => new TaskOverdueModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        TaskLabel = taskLabel,
        ProjectTitle = "Project Alpha",
        Deadline = new DateOnly(2026, 10, 5).ToString("d MMM yyyy", CultureInfo.CurrentCulture),
        Link = taskLink
    });

    await RenderAsync("AddedToProjectTemplate.cshtml", "AddedToProject", () => new AddedToProjectModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        ActorName = "Leyla Mammadova",
        ProjectTitle = "Project Alpha",
        Link = "http://localhost:4200/projects/preview-project"
    });
    }
    finally
    {
        CultureInfo.CurrentCulture = previousCulture;
        CultureInfo.CurrentUICulture = previousUiCulture;
    }
}

Console.WriteLine("Email previews generated:");
foreach (var path in generated)
{
    Console.WriteLine(path);
}

async Task RenderAsync<TModel>(string templateFileName, string outputName, Func<TModel> model)
{
    var templatePath = Path.Combine(templatesDirectory, templateFileName);
    var outputPath = Path.Combine(outputDirectory, $"{outputName}.{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}.html");
    var template = await File.ReadAllTextAsync(templatePath);
    var html = await renderer.ParseAsync(template, model(), true);
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
