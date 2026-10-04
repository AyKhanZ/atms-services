using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Validation.Profile;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Admin.Services.Tests.Validators.Profile;

public class UpdateSettingsValidatorTest : BaseValidatorTest
{
    private UpdateSettingsValidator CreateValidator() => new(
        new ConfigurationBuilder().Build(),
        DictionariesRepositoryMock.Object,
        UserRepositoryMock.Object,
        CurrentUserMock.Object);

    private void SetupValidData()
    {
        DictionariesRepositoryMock.Setup(x => x.IsGenderExistAsync(It.IsAny<Expression<Func<Gender, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        DictionariesRepositoryMock.Setup(x => x.IsMaritalStatusExistAsync(It.IsAny<Expression<Func<MaritalStatus, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        DictionariesRepositoryMock.Setup(x => x.IsLanguageExistAsync(It.IsAny<Expression<Func<Language, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        UserRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { AvatarPath = "users/avatar.webp" });
    }

    private static UpdateSettingsCommand Command() => new()
    {
        Name = "John",
        Surname = "Doe",
        PhoneNumber = "+994501234567",
        Position = "Developer",
        BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
        GenderId = 1,
        MaritalStatusId = 1,
        LanguageId = 1
    };

    [Fact]
    public async Task Validate_OnboardingLimitsAndExistingAvatar_Succeeds()
    {
        SetupValidData();
        var command = Command();
        command.Position = new string('A', 100);

        var result = await CreateValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(101, "Position")]
    [InlineData(51, "Name")]
    public async Task Validate_TooLongValue_ReturnsFieldError(int length, string field)
    {
        SetupValidData();
        var command = Command();
        if (field == "Name") command.Name = new string('A', length);
        else command.Position = new string('A', length);

        var result = await CreateValidator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Fact]
    public async Task Validate_NoExistingOrReplacementAvatar_ReturnsAvatarError()
    {
        SetupValidData();
        UserRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { AvatarPath = "" });

        var result = await CreateValidator().ValidateAsync(Command());

        Assert.Contains(result.Errors, error => error.PropertyName == "Avatar");
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Surname")]
    [InlineData("PhoneNumber")]
    [InlineData("Position")]
    public async Task Validate_EmptyRequiredText_ReturnsFieldError(string field)
    {
        SetupValidData();
        var command = Command();
        switch (field)
        {
            case "Name": command.Name = ""; break;
            case "Surname": command.Surname = ""; break;
            case "PhoneNumber": command.PhoneNumber = ""; break;
            case "Position": command.Position = ""; break;
        }

        var result = await CreateValidator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Theory]
    [InlineData("PhoneNumber", "invalid")]
    [InlineData("Surname", "long")]
    public async Task Validate_InvalidText_ReturnsFieldError(string field, string value)
    {
        SetupValidData();
        var command = Command();
        if (field == "PhoneNumber") command.PhoneNumber = value;
        else command.Surname = new string('A', 101);

        var result = await CreateValidator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Theory]
    [InlineData(-101)]
    [InlineData(-17)]
    [InlineData(1)]
    public async Task Validate_BirthDateOutsideOnboardingRange_ReturnsError(int yearsFromToday)
    {
        SetupValidData();
        var command = Command();
        command.BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(yearsFromToday));

        var result = await CreateValidator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "BirthDate");
    }

    [Theory]
    [InlineData("GenderId")]
    [InlineData("MaritalStatusId")]
    [InlineData("LanguageId")]
    public async Task Validate_UnknownDictionaryValue_ReturnsFieldError(string field)
    {
        SetupValidData();
        var command = Command();
        switch (field)
        {
            case "GenderId": command.GenderId = 999; break;
            case "MaritalStatusId": command.MaritalStatusId = 999; break;
            case "LanguageId": command.LanguageId = 999; break;
        }

        DictionariesRepositoryMock.Setup(x => x.IsGenderExistAsync(It.IsAny<Expression<Func<Gender, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(field != "GenderId");
        DictionariesRepositoryMock.Setup(x => x.IsMaritalStatusExistAsync(It.IsAny<Expression<Func<MaritalStatus, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(field != "MaritalStatusId");
        DictionariesRepositoryMock.Setup(x => x.IsLanguageExistAsync(It.IsAny<Expression<Func<Language, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(field != "LanguageId");

        var result = await CreateValidator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Theory]
    [InlineData("image/gif", 1024L)]
    [InlineData("image/png", 5_242_881L)]
    [InlineData("image/png", 0L)]
    public async Task Validate_InvalidAvatar_ReturnsAvatarError(string contentType, long length)
    {
        SetupValidData();
        var file = new Mock<IFormFile>();
        file.SetupGet(x => x.ContentType).Returns(contentType);
        file.SetupGet(x => x.Length).Returns(length);
        var command = Command();
        command.Avatar = file.Object;

        var result = await CreateValidator().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Avatar");
    }
}
