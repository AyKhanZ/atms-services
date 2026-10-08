using System.ComponentModel.DataAnnotations;
using ATMS.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ATMS.Infrastructure.Tests.Extensions;

public class OptionsExtensionsTest
{
    [Fact]
    public void AddRequiredOptions_WhenSectionExists_BindsIt()
    {
        using var provider = Build(new Dictionary<string, string?> { ["SampleOptions:Url"] = "http://localhost" });

        Assert.Equal("http://localhost", provider.GetRequiredService<IOptions<SampleOptions>>().Value.Url);
    }

    [Fact]
    public void AddRequiredOptions_WhenSectionIsMissing_FailsWithItsName()
    {
        using var provider = Build([]);

        var error = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<SampleOptions>>().Value);

        Assert.Contains(nameof(SampleOptions), error.Message);
    }

    [Fact]
    public void AddRequiredOptions_WhenRequiredValueIsEmpty_FailsWithItsName()
    {
        using var provider = Build(new Dictionary<string, string?> { ["SampleOptions:Url"] = "" });

        var error = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<SampleOptions>>().Value);

        Assert.Contains(nameof(SampleOptions.Url), error.Message);
    }

    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddRequiredOptions<SampleOptions>()
            .BuildServiceProvider();
    }

    public sealed class SampleOptions
    {
        [Required]
        public string? Url { get; init; }
    }
}
