using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Onboarding;
using ATMS.Application.Exceptions.Conflict;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Admin.Services.Tests.Validators.Onboarding;

public sealed class SavePersonalInfoValidatorTest : BaseValidatorTest
{
    [Fact]
    public async Task Validate_WhenVersionDoesNotMatch_ThrowsConflictException()
    {
        SetupDependencies();
        var validator = CreateValidator();
        var command = CreateValidCommand();
        command.Version = -1;

        var exception = await Assert.ThrowsAsync<ConflictException>(() => validator.ValidateAsync(command));

        Assert.Equal(OnboardingMessages.OnboardingConcurrencyConflict, exception.Message);
    }

    [Theory]
    [InlineData("gender")]
    [InlineData("marital-status")]
    [InlineData("language")]
    public async Task Validate_WhenDictionaryValueDoesNotExist_ReturnsFieldValidationFailure(string dictionary)
    {
        SetupDependencies();
        SetupDictionary(dictionary, false);
        var validator = CreateValidator();
        var command = CreateValidCommand();

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == GetPropertyName(dictionary));
    }

    private SavePersonalInfoValidator CreateValidator()
    {
        return new SavePersonalInfoValidator(
            CreateImagesConfiguration(),
            CurrentUserMock.Object,
            OnboardingRepositoryMock.Object,
            DictionariesRepositoryMock.Object);
    }

    private void SetupDependencies()
    {
        CurrentUserMock.SetupGet(x => x.Id).Returns(Guid.NewGuid());
        OnboardingRepositoryMock
            .Setup(x => x.GetAsNoTrackingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OnboardingProgress
            {
                User = new User(),
                PersonalInfo = new OnboardingPersonalInfo
                {
                    AvatarPath = "users/avatar.webp"
                }
            });

        SetupDictionary("gender", true);
        SetupDictionary("marital-status", true);
        SetupDictionary("language", true);
    }

    private void SetupDictionary(string dictionary, bool exists)
    {
        switch (dictionary)
        {
            case "gender":
                DictionariesRepositoryMock
                    .Setup(x => x.IsGenderExistAsync(
                        It.IsAny<Expression<Func<Gender, bool>>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(exists);
                break;
            case "marital-status":
                DictionariesRepositoryMock
                    .Setup(x => x.IsMaritalStatusExistAsync(
                        It.IsAny<Expression<Func<MaritalStatus, bool>>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(exists);
                break;
            case "language":
                DictionariesRepositoryMock
                    .Setup(x => x.IsLanguageExistAsync(
                        It.IsAny<Expression<Func<Language, bool>>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(exists);
                break;
        }
    }

    private static SavePersonalInfoCommand CreateValidCommand()
    {
        return new SavePersonalInfoCommand
        {
            Name = "Aykhan",
            Surname = "Zeynalov",
            PhoneNumber = "+994501234567",
            Position = "Developer",
            LanguageId = 1,
            BirthDate = new DateOnly(1995, 1, 1),
            GenderId = 1,
            MaritalStatusId = 1,
            Version = 0
        };
    }

    private static IConfiguration CreateImagesConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ImagesOptions:ImagesRootPath"] = "images",
                ["ImagesOptions:BaseImageUrl"] = "http://localhost/images"
            })
            .Build();
    }

    private static string GetPropertyName(string dictionary)
    {
        return dictionary switch
        {
            "gender" => nameof(SavePersonalInfoCommand.GenderId),
            "marital-status" => nameof(SavePersonalInfoCommand.MaritalStatusId),
            "language" => nameof(SavePersonalInfoCommand.LanguageId),
            _ => throw new ArgumentOutOfRangeException(nameof(dictionary))
        };
    }
}
