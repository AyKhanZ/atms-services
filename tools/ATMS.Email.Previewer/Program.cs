using ATMS.Email.Models;
using FluentEmail.Razor;

var repositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory);
var templatesDirectory = Path.Combine(repositoryRoot, "src", "ATMS.Shared", "ATMS.Email", "Templates");
var outputDirectory = Path.Combine(repositoryRoot, "artifacts", "email-preview");

Directory.CreateDirectory(outputDirectory);

var renderer = new RazorRenderer();

await RenderTemplateAsync(
    "InviteTemplate.cshtml",
    "confirm-email.html",
    new InviteModel
    {
        Name = "Aykhan",
        Surname = "Zeynalov",
        Email = "aykhan.zeynalov@baim.az",
        Password = "Baim@2026!",
        Link = "https://localhost:7117/api/v1/account/confirm?token=preview-confirmation-token",
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
        Link = "http://localhost:3000/reset-password?token=preview-reset-token",
        DeadlineOfToken = DateTime.Now.AddHours(1)
    });

Console.WriteLine("Email previews generated:");
Console.WriteLine(Path.Combine(outputDirectory, "confirm-email.html"));
Console.WriteLine(Path.Combine(outputDirectory, "forgot-password.html"));

async Task RenderTemplateAsync<TModel>(string templateFileName, string outputFileName, TModel model)
{
    var templatePath = Path.Combine(templatesDirectory, templateFileName);
    var outputPath = Path.Combine(outputDirectory, outputFileName);
    var template = await File.ReadAllTextAsync(templatePath);
    var html = await renderer.ParseAsync(template, model, true);
    await File.WriteAllTextAsync(outputPath, html);
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
