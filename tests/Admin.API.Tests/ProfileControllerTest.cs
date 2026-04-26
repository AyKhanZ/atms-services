using ATMS.Admin.API.Controllers.v1;
using ATMS.Admin.Contracts.Commands.Profile;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Admin.API.Tests;

public class ProfileControllerTest : BaseControllerTest
{
    private readonly ProfileController _controller;

    public ProfileControllerTest()
    {
        _controller = new ProfileController(MediatorMock.Object);
    }

    [Fact]
    public async Task UpdateSettings_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateSettingsCommand
        {
            Id = userId,
            Name = Faker.Name.FullName(),
            Surname = Faker.Name.FullName(),
            GenderId = Faker.Random.Int(1, 4),
            MaritalStatusId = Faker.Random.Int(1, 4),
            Position = Faker.Company.CompanyName(),
            PhoneNumber = Faker.Phone.PhoneNumber(),
            BirthDate = Faker.Date.Recent(),
        };

        MediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateSettingsCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateSettings(userId, command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        MediatorMock.Verify(m => m.Send(
            It.Is<UpdateSettingsCommand>(c => c.Id == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePhoto_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdatePhotoCommand { Id = userId, FileName = Faker.Random.AlphaNumeric(10) };

        MediatorMock
            .Setup(m => m.Send(It.IsAny<UpdatePhotoCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdatePhoto(userId, command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        MediatorMock.Verify(m => m.Send(
            It.Is<UpdatePhotoCommand>(c => c.Id == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLanguage_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateLanguageCommand { Id = userId, Language = Faker.Random.AlphaNumeric(10) };

        MediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateLanguageCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateLanguage(userId, command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        MediatorMock.Verify(m => m.Send(
            It.Is<UpdateLanguageCommand>(c => c.Id == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}