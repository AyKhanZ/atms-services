using ATMS.Swagger.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ATMS.Swagger.Tests;

public class RateLimitOptionsTests
{
    [Fact]
    public void ReadOptions_FullSection_ReadsEveryLimit()
    {
        var options = RateLimitingExtensions.ReadOptions(Configuration(FullSection()));

        Assert.True(options.Enabled);
        Assert.Equal(300, options.UserRequests.TokenLimit);
        Assert.Equal(6, options.AuthLogin.SegmentsPerWindow);
        Assert.Equal(500, options.Creates.DailyLimit);
        Assert.Equal(0, options.Migrations.QueueLimit);
    }

    [Fact]
    public void ReadOptions_MissingSection_Throws()
    {
        var exception = Assert.Throws<OptionsValidationException>(() =>
            RateLimitingExtensions.ReadOptions(Configuration([])));

        Assert.Contains("RateLimitOptions section is missing.", exception.Failures);
    }

    [Fact]
    public void ReadOptions_MissingNumber_NamesIt()
    {
        var section = FullSection();
        section.Remove("RateLimitOptions:Creates:DailyLimit");

        var exception = Assert.Throws<OptionsValidationException>(() =>
            RateLimitingExtensions.ReadOptions(Configuration(section)));

        Assert.Contains(exception.Failures, failure => failure.StartsWith("RateLimitOptions:Creates:DailyLimit:"));
    }

    [Fact]
    public void ReadOptions_MissingPolicy_NamesIt()
    {
        var section = FullSection();
        section.Remove("RateLimitOptions:Heavy:PermitLimit");
        section.Remove("RateLimitOptions:Heavy:QueueLimit");

        var exception = Assert.Throws<OptionsValidationException>(() =>
            RateLimitingExtensions.ReadOptions(Configuration(section)));

        Assert.Contains(exception.Failures, failure => failure.StartsWith("RateLimitOptions:Heavy:"));
    }

    [Fact]
    public void ReadOptions_MissingEnabled_NamesIt()
    {
        var section = FullSection();
        section.Remove("RateLimitOptions:Enabled");

        var exception = Assert.Throws<OptionsValidationException>(() =>
            RateLimitingExtensions.ReadOptions(Configuration(section)));

        Assert.Contains(exception.Failures, failure => failure.StartsWith("RateLimitOptions:Enabled:"));
    }

    // both APIs carry their own copy of the section; a typo in one would only show up on its start
    [Fact]
    public void AppSettings_BothApis_HaveTheSameValidLimits()
    {
        var admin = AppSettings("src/ATMS.Admin/ATMS.Admin.API/appsettings.json");
        var project = AppSettings("src/ATMS.Project/ATMS.Project.API/appsettings.json");

        RateLimitingExtensions.ReadOptions(admin);
        RateLimitingExtensions.ReadOptions(project);
        Assert.Equal(Flatten(admin), Flatten(project));
    }

    private static Dictionary<string, string?> FullSection() => new()
    {
        ["RateLimitOptions:Enabled"] = "true",
        ["RateLimitOptions:UserRequests:TokenLimit"] = "300",
        ["RateLimitOptions:UserRequests:TokensPerPeriod"] = "5",
        ["RateLimitOptions:UserRequests:PeriodSeconds"] = "1",
        ["RateLimitOptions:IpRequests:TokenLimit"] = "100",
        ["RateLimitOptions:IpRequests:TokensPerPeriod"] = "2",
        ["RateLimitOptions:IpRequests:PeriodSeconds"] = "1",
        ["RateLimitOptions:Mutations:TokenLimit"] = "60",
        ["RateLimitOptions:Mutations:TokensPerPeriod"] = "1",
        ["RateLimitOptions:Mutations:PeriodSeconds"] = "1",
        ["RateLimitOptions:AuthLogin:PermitLimit"] = "20",
        ["RateLimitOptions:AuthLogin:WindowSeconds"] = "60",
        ["RateLimitOptions:AuthLogin:SegmentsPerWindow"] = "6",
        ["RateLimitOptions:AuthRefresh:PermitLimit"] = "60",
        ["RateLimitOptions:AuthRefresh:WindowSeconds"] = "60",
        ["RateLimitOptions:AuthRefresh:SegmentsPerWindow"] = "6",
        ["RateLimitOptions:AuthEmail:PermitLimit"] = "5",
        ["RateLimitOptions:AuthEmail:WindowSeconds"] = "900",
        ["RateLimitOptions:AuthToken:PermitLimit"] = "20",
        ["RateLimitOptions:AuthToken:WindowSeconds"] = "900",
        ["RateLimitOptions:Creates:TokenLimit"] = "30",
        ["RateLimitOptions:Creates:TokensPerPeriod"] = "1",
        ["RateLimitOptions:Creates:PeriodSeconds"] = "3",
        ["RateLimitOptions:Creates:DailyLimit"] = "500",
        ["RateLimitOptions:Emails:TokenLimit"] = "10",
        ["RateLimitOptions:Emails:TokensPerPeriod"] = "1",
        ["RateLimitOptions:Emails:PeriodSeconds"] = "180",
        ["RateLimitOptions:Emails:DailyLimit"] = "100",
        ["RateLimitOptions:Uploads:TokenLimit"] = "20",
        ["RateLimitOptions:Uploads:TokensPerPeriod"] = "1",
        ["RateLimitOptions:Uploads:PeriodSeconds"] = "6",
        ["RateLimitOptions:Uploads:DailyLimit"] = "300",
        ["RateLimitOptions:Downloads:PermitLimit"] = "6",
        ["RateLimitOptions:Downloads:QueueLimit"] = "6",
        ["RateLimitOptions:Heavy:PermitLimit"] = "4",
        ["RateLimitOptions:Heavy:QueueLimit"] = "4",
        ["RateLimitOptions:Migrations:PermitLimit"] = "1",
        ["RateLimitOptions:Migrations:QueueLimit"] = "0"
    };

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static IConfiguration AppSettings(string relativePath)
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "atms-services.sln")))
        {
            root = root.Parent;
        }

        Assert.NotNull(root);
        return new ConfigurationBuilder().AddJsonFile(Path.Combine(root.FullName, relativePath)).Build();
    }

    private static Dictionary<string, string?> Flatten(IConfiguration configuration) =>
        configuration.GetSection("RateLimitOptions").AsEnumerable().ToDictionary(pair => pair.Key, pair => pair.Value);
}
