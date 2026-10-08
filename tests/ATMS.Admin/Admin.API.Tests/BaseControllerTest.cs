using Bogus;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Admin.API.Tests;

public class BaseControllerTest
{
    protected readonly Faker Faker = new();
    protected readonly Mock<IMediator> MediatorMock = new();

    protected static IConfiguration BuildConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["RedirectUrlOptions:BaseUrl"] = "https://",
            ["RedirectUrlOptions:ResetPasswordPage"] = "https://reset",
            ["RedirectUrlOptions:EmailConfirmedPage"] = "https://ok",
            ["RedirectUrlOptions:EmailAlreadyConfirmedPage"] = "https://already-ok",
            ["RedirectUrlOptions:EmailConfirmFailedPage"] = "https://fail"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
