using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Admin.Contracts.Requests.Profile;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class ProfileControllerTest : BaseControllerTest
{
    [Fact]
    public void Route_DoesNotAcceptUserId()
    {
        var route = (RouteAttribute?)Attribute.GetCustomAttribute(typeof(ProfileController), typeof(RouteAttribute));

        Assert.Equal("api/v1/profile", route?.Template);
    }

    private ProfileController CreateController() => new(MediatorMock.Object);

    private static ProfileModel Model() => new()
    {
        Name = "Jane", Surname = "Doe", Email = "jane@example.com", PhoneNumber = "+994501234567",
        Position = "Developer", AvatarPath = "avatar.webp", LanguageId = 1,
        BirthDate = new DateOnly(1990, 1, 1), GenderId = 1, MaritalStatusId = 1
    };

    [Fact]
    public async Task Get_ReturnsCurrentProfile()
    {
        var model = Model();
        MediatorMock.Setup(x => x.Send(It.IsAny<GetProfileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var result = await CreateController().Get(CancellationToken.None);

        Assert.Same(model, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task UpdateSettings_ReturnsUpdatedProfile()
    {
        var command = new UpdateSettingsCommand
        {
            Name = "Jane", Surname = "Doe", PhoneNumber = "+994501234567", Position = "Developer",
            BirthDate = new DateOnly(1990, 1, 1), GenderId = 1, MaritalStatusId = 1, LanguageId = 1
        };
        var model = Model();
        MediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var result = await CreateController().UpdateSettings(command, CancellationToken.None);

        Assert.Same(model, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task UpdateLanguage_ReturnsNoContent()
    {
        var command = new UpdateLanguageCommand { Language = "AZ" };
        MediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await CreateController().UpdateLanguage(command, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
