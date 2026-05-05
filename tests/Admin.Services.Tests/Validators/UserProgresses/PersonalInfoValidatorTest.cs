using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Validation.UserProgresses;
using FluentValidation.TestHelper;
using Moq;

namespace Admin.Services.Tests.Validators.UserProgresses;

public class PersonalInfoValidatorTest : BaseValidatorTest
{
    private readonly PersonalInfoValidator _validator;

    public PersonalInfoValidatorTest()
    {
        _validator = new PersonalInfoValidator(DictionariesRepositoryMock.Object);
    }

    [Fact]
    public async Task Should_Fail_When_Phone_Invalid()
    {
        var cmd = BuildValid();
        cmd.PhoneNumber = "123";

        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public async Task Should_Fail_When_Gender_Not_Exist()
    {
        var cmd = BuildValid();

        DictionariesRepositoryMock.Setup(x =>
                x.IsGenderExistAsync(It.IsAny<Expression<Func<Gender, bool>>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.GenderId);
    }

    [Fact]
    public async Task Should_Pass_When_Valid()
    {
        var cmd = BuildValid();

        DictionariesRepositoryMock.Setup(x => x.IsGenderExistAsync(It.IsAny<Expression<Func<Gender, bool>>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(true);

        DictionariesRepositoryMock.Setup(x => x.IsMaritalStatusExistAsync(It.IsAny<Expression<Func<MaritalStatus, bool>>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private PersonalInfoCommand BuildValid() => new()
    {
        Name = "John",
        Surname = "Doe",
        Email = "test@test.com",
        PhoneNumber = "+994501234567",
        Position = "Dev",
        Language = "en",
        AvatarPath = "avatar.png",
        BirthDate = DateTime.UtcNow.AddYears(-20),
        GenderId = 1,
        MaritalStatusId = 1
    };
}